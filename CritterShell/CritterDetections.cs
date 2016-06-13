using CritterShell.Critters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace CritterShell
{
    internal class CritterDetections : CsvReaderWriter
    {
        public static readonly ReadOnlyCollection<string> CsvColumns;

        public List<CritterDetection> Detections { get; private set; }

        static CritterDetections()
        {
            CritterDetections.CsvColumns = new List<string>()
            {
                Constant.DetectionColumn.Station,
                Constant.DetectionColumn.File,
                Constant.DetectionColumn.RelativePath,
                Constant.DetectionColumn.StartTime,
                Constant.DetectionColumn.EndTime,
                Constant.DetectionColumn.Duration,
                Constant.DetectionColumn.TriggerSource,
                Constant.DetectionColumn.Identification,
                Constant.DetectionColumn.Confidence,
                Constant.DetectionColumn.GroupType,
                Constant.DetectionColumn.Age,
                Constant.DetectionColumn.Pelage,
                Constant.DetectionColumn.Activity,
                Constant.DetectionColumn.Comments,
                Constant.DetectionColumn.Folder,
                Constant.DetectionColumn.Survey
            }.AsReadOnly();
        }

        public CritterDetections(CritterImages critters, TimeSpan window, bool bySite)
        {
            this.Detections = new List<CritterDetection>();

            foreach (IGrouping<string, CritterImage> imagesFromStation in critters.Images.GroupBy(image => bySite ? image.GetSite() : image.Station))
            {
                if (String.IsNullOrWhiteSpace(imagesFromStation.Key))
                {
                    continue;
                }

                foreach (IGrouping<string, CritterImage> imagesOfSpecies in imagesFromStation.GroupBy(image => image.Identification))
                {
                    if (String.IsNullOrWhiteSpace(imagesOfSpecies.Key))
                    {
                        continue;
                    }

                    CritterDetection previousDetection = null;
                    foreach (CritterImage image in imagesOfSpecies.OrderBy(image => image.Time))
                    {
                        CritterDetection detection = new CritterDetection(image);
                        if (bySite)
                        {
                            detection.Station = image.GetSite();
                        }

                        // assume camera clock is in the same time zone as this computer
                        if (detection.StartTime.Kind == DateTimeKind.Local)
                        {
                            detection.StartTime = detection.StartTime.ToUniversalTime();
                        }
                        if (detection.EndTime.Kind == DateTimeKind.Local)
                        {
                            detection.EndTime = detection.StartTime.ToUniversalTime();
                        }

                        if (previousDetection == null || previousDetection.TryMerge(detection, window) == false)
                        {
                            this.Detections.Add(detection);
                            previousDetection = detection;
                        }
                    }
                }
            }
        }

        public void WriteCsv(string filePath)
        {
            using (TextWriter fileWriter = new StreamWriter(filePath, false))
            {
                // Write the header as defined by the data labels in the template file
                // If the data label is an empty string, we use the label instead.
                // The append sequence results in a trailing comma which is retained when writing the line.
                StringBuilder header = new StringBuilder();
                foreach (string columnName in CritterDetections.CsvColumns)
                {
                    header.Append(this.AddColumnValue(columnName));
                }
                fileWriter.WriteLine(header.ToString());

                // For each row in the data table, write out the columns in the same order as the 
                // data labels in the template file
                foreach (CritterDetection detection in this.Detections)
                {
                    StringBuilder row = new StringBuilder();
                    row.Append(this.AddColumnValue(detection.Station));
                    row.Append(this.AddColumnValue(detection.File));
                    row.Append(this.AddColumnValue(detection.RelativePath));
                    row.Append(this.AddColumnValue(detection.StartTime));
                    row.Append(this.AddColumnValue(detection.EndTime));
                    row.Append(this.AddColumnValue(detection.Duration.ToString().ToLowerInvariant()));
                    row.Append(this.AddColumnValue(detection.TriggerSource.ToString().ToLowerInvariant()));
                    row.Append(this.AddColumnValue(detection.Identification));
                    row.Append(this.AddColumnValue(detection.Confidence.ToString().ToLowerInvariant()));
                    row.Append(this.AddColumnValue(detection.GroupType.ToString().ToLowerInvariant()));
                    row.Append(this.AddColumnValue(detection.Age.ToString().ToLowerInvariant()));
                    row.Append(this.AddColumnValue(detection.Pelage));
                    row.Append(this.AddColumnValue(detection.Activity.ToString().ToLowerInvariant()));
                    row.Append(this.AddColumnValue(detection.Comments));
                    row.Append(this.AddColumnValue(detection.Folder));
                    row.Append(this.AddColumnValue(detection.Survey));
                    fileWriter.WriteLine(row.ToString());
                }
            }
        }
    }
}
