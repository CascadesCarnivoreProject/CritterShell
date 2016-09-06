using CritterShell.Critters;
using CritterShell.Gpx;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace CritterShell.UnitTests
{
    [TestClass]
    public class Conversions
    {
        [TestMethod]
        public void ConvertGpxToCsv()
        {
            string gpxFileName = TestConstant.File.CarnivoreSign;
            GpxFile gpxFile = new GpxFile(gpxFileName);
            CritterSigns critterSign = new CritterSigns(gpxFile);

            string csvFilePath = Path.Combine(Path.GetDirectoryName(gpxFileName), Path.GetFileNameWithoutExtension(gpxFileName) + Constant.Csv.Extension);
            critterSign.WriteCsv(csvFilePath);
        }

        [TestMethod]
        public void GetDielAndMonthlyActivity()
        {
            // get detections
            string imageFileName = TestConstant.File.CarnivoreImages;
            CritterImages critterImages = new CritterImages();
            List<string> importErrors;
            Assert.IsTrue(critterImages.TryReadCsv(imageFileName, out importErrors));
            Assert.IsTrue(importErrors.Count == 0);

            CritterDetections critterDetections = new CritterDetections(critterImages, Constant.DefaultDetectionMergeWindow, false);
            Assert.IsTrue(critterDetections.Detections.Count < 0.5 * critterImages.Images.Count);
            string detectionFilePath = Path.Combine(Path.GetDirectoryName(imageFileName), Path.GetFileNameWithoutExtension(imageFileName) + "-DetectionsByStation" + Path.GetExtension(imageFileName));
            critterDetections.WriteCsv(detectionFilePath);

            critterDetections = new CritterDetections(critterImages, TimeSpan.FromDays(30), true);
            Assert.IsTrue(critterDetections.Detections.Count < 0.1 * critterImages.Images.Count);
            detectionFilePath = Path.Combine(Path.GetDirectoryName(imageFileName), Path.GetFileNameWithoutExtension(imageFileName) + "-DetectionsBySite" + Path.GetExtension(imageFileName));
            critterDetections.WriteCsv(detectionFilePath);

            // extract diel activity from detections
            CritterDetections detectionsForDielActivity = new CritterDetections();
            Assert.IsTrue(detectionsForDielActivity.TryReadCsv(detectionFilePath, out importErrors));
            Assert.IsTrue(detectionsForDielActivity.Detections.Count == critterDetections.Detections.Count);
            Assert.IsTrue(importErrors.Count == 0);

            ActivityObservations<CritterDielActivity> dielActivity = new ActivityObservations<CritterDielActivity>(critterDetections);
            string dielFilePath = Path.Combine(Path.GetDirectoryName(imageFileName), Path.GetFileNameWithoutExtension(imageFileName) + "-DielActivityBySite" + Path.GetExtension(imageFileName));
            dielActivity.WriteCsv(dielFilePath);

            // extract monthly activity from detections
            CritterDetections detectionsForMonthlyActivity = new CritterDetections();
            Assert.IsTrue(detectionsForMonthlyActivity.TryReadCsv(detectionFilePath, out importErrors));
            Assert.IsTrue(detectionsForMonthlyActivity.Detections.Count == critterDetections.Detections.Count);
            Assert.IsTrue(importErrors.Count == 0);

            ActivityObservations<CritterMonthlyActivity> monthlyActivity = new ActivityObservations<CritterMonthlyActivity>(critterDetections);
            string monthlyFilePath = Path.Combine(Path.GetDirectoryName(imageFileName), Path.GetFileNameWithoutExtension(imageFileName) + "-MonthlyActivityBySite" + Path.GetExtension(imageFileName));
            monthlyActivity.WriteCsv(monthlyFilePath);
        }
    }
}
