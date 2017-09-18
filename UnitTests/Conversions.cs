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
        public void ConvertCritterGpx()
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
        public void ConvertWaypoints()
        {
            string gpxFileName = TestConstant.File.CalochortusMacrocarpus;
            GpxFile gpxFile = new GpxFile(gpxFileName);
            GpxSpreadsheet spreadsheet = new GpxSpreadsheet(gpxFile);

            string csvFilePath = Path.Combine(Path.GetDirectoryName(gpxFileName), Path.GetFileNameWithoutExtension(gpxFileName) + Constant.Csv.Extension);
            spreadsheet.WriteCsv(csvFilePath);

            string xlsxFilePath = Path.Combine(Path.GetDirectoryName(gpxFileName), Path.GetFileNameWithoutExtension(gpxFileName) + Constant.Excel.Extension);
            spreadsheet.WriteXlsx(xlsxFilePath, "waypoints");
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

            bool bySite = true;
            critterDetections = new CritterDetections(critterImages, TimeSpan.FromDays(30), bySite);
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

            Dictionary<string, List<string>> groups = new Dictionary<string, List<string>>() { { "DS02 group", new List<string>() { "DS02" } } };

            StationData stations = new StationData();
            FileReadResult stationReadResult = stations.TryRead(TestConstant.File.StationData, Constant.Excel.DefaultStationsWorksheetName);
            Assert.IsFalse(stationReadResult.Failed);
            Assert.IsTrue(stationReadResult.Verbose.Count == 0);
            Assert.IsTrue(stationReadResult.Warnings.Count == 0);

            StationData sites = stations.GetSites();

            ActivityObservations<CritterDielActivity> dielActivity = new ActivityObservations<CritterDielActivity>(critterDetections, sites, groups)
            {
                WriteProbabilities = true
            };
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

            ActivityObservations<CritterMonthlyActivity> monthlyActivity = new ActivityObservations<CritterMonthlyActivity>(critterDetections, sites, null)
            {
                WriteProbabilities = false
            };
            string monthlyActivityCsvFilePath = Path.Combine(Path.GetDirectoryName(csvFileName), Path.GetFileNameWithoutExtension(csvFileName) + "-MonthlyActivityBySite" + Path.GetExtension(csvFileName));
            monthlyActivity.WriteCsv(monthlyActivityCsvFilePath);
            monthlyActivity.WriteXlsx(commonXlsxFilePath, "site activity");
        }
    }
}
