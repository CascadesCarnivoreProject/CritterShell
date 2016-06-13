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
        public void GetDetections()
        {
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
        }
    }
}
