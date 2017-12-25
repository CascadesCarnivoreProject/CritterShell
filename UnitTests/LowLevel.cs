using CritterShell.Critters;
using CritterShell.Images;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CritterShell.UnitTests
{
    [TestClass]
    public class LowLevel
    {
        private static readonly List<int> CurrentYearDaysInMonth;
        private static readonly List<int> PreviousYearDaysInMonth;
        private static readonly DateTime UtcToday;

        public TestContext TestContext { get; set; }

        static LowLevel()
        {
            LowLevel.CurrentYearDaysInMonth = new List<int>(12);
            LowLevel.PreviousYearDaysInMonth = new List<int>(12);
            LowLevel.UtcToday = DateTime.UtcNow;
            for (int month = 1; month <= 12; ++month)
            {
                LowLevel.CurrentYearDaysInMonth.Add(DateTime.DaysInMonth(LowLevel.UtcToday.Year, month));
                LowLevel.PreviousYearDaysInMonth.Add(DateTime.DaysInMonth(LowLevel.UtcToday.Year - 1, month));
            }
        }

        [TestMethod]
        public void ImageProcessing()
        {
            List<string> imageNames = new List<string>() { TestConstant.File.ColorSquirrel, TestConstant.File.GreyscaleSquirrel };
            for (int index = 0; index < imageNames.Count; ++index)
            {
                string imageName = imageNames[index];
                WriteableBitmap image;
                using (FileStream imageStream = new FileStream(imageName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    JpegBitmapDecoder jpegDecoder = new JpegBitmapDecoder(imageStream, BitmapCreateOptions.None, BitmapCacheOption.None);
                    image = new WriteableBitmap(jpegDecoder.Frames[0]);
                }

                // histogram
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                ImageHistogram histogram = new ImageHistogram(image, 256, 100 * (index % 2));
                stopwatch.Stop();
                this.TestContext.WriteLine("{0} histogram in {1} milliseconds.", imageName, stopwatch.ElapsedMilliseconds);

                if ((index % 2) == 0)
                {
                    histogram.WriteCsv(Path.GetFileNameWithoutExtension(imageName) + Constant.Csv.Extension);
                }
                else
                {
                    histogram.WriteXlsx(Path.GetFileNameWithoutExtension(imageName) + Constant.Excel.Extension, histogram.Bins.ToString());
                }

                // rectangle location
                Int32Rect triggerArea = Bushnell.FindTrigger(image);
                Assert.IsTrue(triggerArea == new Int32Rect(234, 2365, 61, 62));

                Int32Rect temperatureArea = Bushnell.FindTemperature(image);
                Assert.IsTrue(temperatureArea == new Int32Rect(1454, 2373, 82, 41));

                // rectangle extraction
                WriteableBitmap rectangle = image.ExtractRectangle(temperatureArea);
                rectangle.Threshold(Constant.Bushnell.BlackPixelThreshold);

                ImageProperties properties = rectangle.GetProperties();
                Assert.IsTrue(properties.BlackPixels >= 0);
                Assert.IsTrue((properties.BlackPixels + properties.WhitePixels) == properties.Pixels);
                Assert.IsTrue(properties.Hash > 0);
                Assert.IsTrue(properties.PixelWidth == temperatureArea.Width);
                Assert.IsTrue(properties.PixelHeight == temperatureArea.Height);
                Assert.IsTrue(properties.WhitePixels >= 0);

                WriteableBitmap converted = rectangle.Convert(PixelFormats.Gray8);
                ImageProperties convertedProperties = converted.GetProperties();
                Assert.IsTrue((convertedProperties.BlackPixels + convertedProperties.WhitePixels) == convertedProperties.Pixels);
                Assert.IsTrue(properties.PixelWidth == convertedProperties.PixelWidth);
                Assert.IsTrue(properties.PixelHeight == convertedProperties.PixelHeight);

                converted.Save(Path.GetFileNameWithoutExtension(imageName) + Constant.File.PngExtension);
            }
        }

        [TestMethod]
        public void StationUptime()
        {
            Station station = new Station("test", "one year", new DateTime(LowLevel.UtcToday.Year, 1, 1), new DateTime(LowLevel.UtcToday.Year, 12, 31));
            this.VerifyUptime(station, LowLevel.CurrentYearDaysInMonth);

            station = new Station("test", "two years", new DateTime(LowLevel.UtcToday.Year - 1, 1, 1), new DateTime(LowLevel.UtcToday.Year, 12, 31));
            this.VerifyUptime(station, LowLevel.CurrentYearDaysInMonth, LowLevel.PreviousYearDaysInMonth);

            int daysInFebruary = DateTime.DaysInMonth(LowLevel.UtcToday.Year, 2);
            station = new Station("test", "February", new DateTime(LowLevel.UtcToday.Year, 2, 1), new DateTime(LowLevel.UtcToday.Year, 2, daysInFebruary));
            this.VerifyUptime(station, new List<int>() { 0, daysInFebruary, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

            station = new Station("test", "partial month", new DateTime(LowLevel.UtcToday.Year, 8, 5), new DateTime(LowLevel.UtcToday.Year, 8, 26));
            this.VerifyUptime(station, new List<int>() { 0, 0, 0, 0, 0, 0, 0, 21, 0, 0, 0, 0 });

            station = new Station("test", "month overlap", new DateTime(LowLevel.UtcToday.Year, 7, 15), new DateTime(LowLevel.UtcToday.Year, 8, 13));
            this.VerifyUptime(station, new List<int>() { 0, 0, 0, 0, 0, 0, 16, 13, 0, 0, 0, 0 });

            station = new Station("test", "year overlap", new DateTime(LowLevel.UtcToday.Year - 1, 11, 06), new DateTime(LowLevel.UtcToday.Year, 5, 21));
            this.VerifyUptime(station, new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 24, 31 }, new List<int>() { 31, daysInFebruary, 31, 30, 21, 0, 0, 0, 0, 0, 0, 0 });
        }

        private void VerifyUptime(Station station, List<int> expectedUptime)
        {
            for (int month = 1; month <= 12; ++month)
            {
                Assert.IsTrue(station.GetUptime(month) == expectedUptime[month - 1]);
            }
        }

        private void VerifyUptime(Station station, List<int> expectedUptime1, List<int> expectedUptime2)
        {
            for (int month = 1; month <= 12; ++month)
            {
                Assert.IsTrue(station.GetUptime(month) == expectedUptime1[month - 1] + expectedUptime2[month - 1]);
            }
        }

        private void VerifyUptime(Station station, List<int> expectedUptime1, List<int> expectedUptime2, List<int> expectedUptime3)
        {
            for (int month = 1; month <= 12; ++month)
            {
                Assert.IsTrue(station.GetUptime(month) == expectedUptime1[month - 1] + expectedUptime2[month - 1] + expectedUptime3[month - 1]);
            }
        }
    }
}
