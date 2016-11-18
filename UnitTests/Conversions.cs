using CritterShell.Critters;
using CritterShell.Gpx;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CritterShell.UnitTests
{
    [TestClass]
    public class Conversions
    {
        [TestMethod]
        public void ConvertGpx()
        {
            string gpxFileName = TestConstant.File.CarnivoreSign;
            GpxFile gpxFile = new GpxFile(gpxFileName);
            CritterSigns critterSign = new CritterSigns(gpxFile);

            string csvFilePath = Path.Combine(Path.GetDirectoryName(gpxFileName), Path.GetFileNameWithoutExtension(gpxFileName) + Constant.Csv.Extension);
            critterSign.WriteCsv(csvFilePath);

            string xlsxFilePath = Path.Combine(Path.GetDirectoryName(gpxFileName), Path.GetFileNameWithoutExtension(gpxFileName) + Constant.Excel.Extension);
            critterSign.WriteXlsx(xlsxFilePath, "critter sign");
        }

        [TestMethod]
        public void GetDielAndMonthlyActivity()
        {
            // get detections
            string csvFileName = TestConstant.File.CarnivoreImages;
            CritterImages critterImages = new CritterImages();
            FileReadResult readResult = critterImages.TryRead(csvFileName, null);
            Assert.IsFalse(readResult.Failed);
            Assert.IsTrue(readResult.Verbose.Count == 0);
            Assert.IsTrue(readResult.Warnings.Count == 0);

            CritterDetections critterDetections = new CritterDetections(critterImages, Constant.DefaultDetectionMergeWindow, false);
            Assert.IsTrue(critterDetections.Detections.Count < 0.5 * critterImages.Images.Count);
            string detectionCsvFilePath = Path.Combine(Path.GetDirectoryName(csvFileName), Path.GetFileNameWithoutExtension(csvFileName) + "-DetectionsByStation" + Constant.Csv.Extension);
            critterDetections.WriteCsv(detectionCsvFilePath);
            string commonXlsxFilePath = Path.Combine(Path.GetDirectoryName(csvFileName), Path.GetFileNameWithoutExtension(csvFileName) + Constant.Excel.Extension);
            critterDetections.WriteXlsx(commonXlsxFilePath, TestConstant.File.StationDetectionsWorksheet);

            critterDetections = new CritterDetections(critterImages, TimeSpan.FromDays(30), true);
            Assert.IsTrue(critterDetections.Detections.Count < 0.1 * critterImages.Images.Count);
            detectionCsvFilePath = Path.Combine(Path.GetDirectoryName(csvFileName), Path.GetFileNameWithoutExtension(csvFileName) + "-DetectionsBySite" + Constant.Csv.Extension);
            critterDetections.WriteCsv(detectionCsvFilePath);
            critterDetections.WriteXlsx(commonXlsxFilePath, TestConstant.File.SiteDetectionsWorksheet);

            // extract diel activity from detections
            CritterDetections detectionsForDielActivity = new CritterDetections();
            readResult = detectionsForDielActivity.TryRead(detectionCsvFilePath, null);
            Assert.IsFalse(readResult.Failed);
            Assert.IsTrue(detectionsForDielActivity.Detections.Count == critterDetections.Detections.Count);
            Assert.IsTrue(readResult.Verbose.Count == 0);
            Assert.IsTrue(readResult.Warnings.Count == 0);

            detectionsForDielActivity = new CritterDetections();
            readResult = detectionsForDielActivity.TryRead(commonXlsxFilePath, TestConstant.File.SiteDetectionsWorksheet);
            Assert.IsFalse(readResult.Failed);
            Assert.IsTrue(detectionsForDielActivity.Detections.Count == critterDetections.Detections.Count);
            Assert.IsTrue(readResult.Verbose.Count == 0);
            Assert.IsTrue(readResult.Warnings.Count == 0);

            TimeZoneInfo solarTimeForLocalTimeZone = TimeZoneInfo.GetSystemTimeZones().Where(timeZone => timeZone.BaseUtcOffset == TimeZoneInfo.Local.BaseUtcOffset && timeZone.SupportsDaylightSavingTime == false).First();
            foreach (CritterDetection detection in critterDetections.Detections)
            {
                if (detection.UtcOffset != solarTimeForLocalTimeZone.BaseUtcOffset)
                {
                    DateTimeOffset startDateTime = detection.GetStartDateTimeOffset();
                    detection.SetStartAndEndDateTimes(startDateTime.SetOffset(solarTimeForLocalTimeZone.BaseUtcOffset));
                }
            }

            Dictionary<string, List<string>> groups = new Dictionary<string, List<string>>();
            groups.Add("DS02 group", new List<string>() { "DS02" });
            ActivityObservations<CritterDielActivity> dielActivity = new ActivityObservations<CritterDielActivity>(critterDetections, groups);
            dielActivity.WriteTotal = true;
            dielActivity.WriteProbabilities = true;
            string dielActivityCsvFilePath = Path.Combine(Path.GetDirectoryName(csvFileName), Path.GetFileNameWithoutExtension(csvFileName) + "-DielActivityBySite" + Path.GetExtension(csvFileName));
            dielActivity.WriteCsv(dielActivityCsvFilePath);
            dielActivity.WriteXlsx(commonXlsxFilePath, "station activity");

            // extract monthly activity from detections
            CritterDetections detectionsForMonthlyActivity = new CritterDetections();
            readResult = detectionsForMonthlyActivity.TryRead(detectionCsvFilePath, null);
            Assert.IsFalse(readResult.Failed);
            Assert.IsTrue(detectionsForMonthlyActivity.Detections.Count == critterDetections.Detections.Count);
            Assert.IsTrue(readResult.Verbose.Count == 0);
            Assert.IsTrue(readResult.Warnings.Count == 0);

            detectionsForMonthlyActivity = new CritterDetections();
            readResult = detectionsForMonthlyActivity.TryRead(commonXlsxFilePath, TestConstant.File.SiteDetectionsWorksheet);
            Assert.IsFalse(readResult.Failed);
            Assert.IsTrue(detectionsForMonthlyActivity.Detections.Count == critterDetections.Detections.Count);
            Assert.IsTrue(readResult.Verbose.Count == 0);
            Assert.IsTrue(readResult.Warnings.Count == 0);

            ActivityObservations<CritterMonthlyActivity> monthlyActivity = new ActivityObservations<CritterMonthlyActivity>(critterDetections, null);
            monthlyActivity.WriteTotal = false;
            monthlyActivity.WriteProbabilities = false;
            string monthlyActivityCsvFilePath = Path.Combine(Path.GetDirectoryName(csvFileName), Path.GetFileNameWithoutExtension(csvFileName) + "-MonthlyActivityBySite" + Path.GetExtension(csvFileName));
            monthlyActivity.WriteCsv(monthlyActivityCsvFilePath);
            monthlyActivity.WriteXlsx(commonXlsxFilePath, "site activity");
        }
    }
}
