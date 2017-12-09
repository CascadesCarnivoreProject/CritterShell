﻿using CritterShell.Critters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
        public void ImageHistogram()
        {
            List<string> imageNames = new List<string>() { TestConstant.File.ColorSquirrel, TestConstant.File.GreyscaleSquirrel };
            for (int image = 0; image < imageNames.Count; ++image)
            {
                string imageName = imageNames[image];
                GetHistogram histogramCmdlet = new GetHistogram()
                {
                    BottomRowsToSkip = 100 * (image % 2),
                    Image = imageName
                };

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                ImageHistogram histogram = histogramCmdlet.Invoke<ImageHistogram>().Single();
                stopwatch.Stop();
                this.TestContext.WriteLine("{0} histogram in {1} milliseconds.", imageName, stopwatch.ElapsedMilliseconds);

                if ((image % 2) == 0)
                {
                    histogram.WriteCsv(Path.GetFileNameWithoutExtension(imageName) + Constant.Csv.Extension);
                }
                else
                {
                    histogram.WriteXlsx(Path.GetFileNameWithoutExtension(imageName) + Constant.Excel.Extension, histogram.Bins.ToString());
                }
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
