using CritterShell.Critters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace CritterShell
{
    internal class CritterDetections : CsvReaderWriter
    {
        private static readonly ReadOnlyCollection<string> CsvColumns;

        public List<CritterDetection> Detections { get; private set; }

        static CritterDetections()
        {
            CritterDetections.CsvColumns = new List<string>()
            {
                Constant.DetectionColumn.Station,
                Constant.DetectionColumn.File,
                Constant.DetectionColumn.RelativePath,
                Constant.DetectionColumn.StartDateTime,
                Constant.DetectionColumn.EndDateTime,
                Constant.DetectionColumn.UtcOffset,
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

        public CritterDetections()
        {
            this.Detections = new List<CritterDetection>();
        }

        public CritterDetections(CritterImages critters, TimeSpan window, bool bySite)
            : this()
        {
            foreach (IGrouping<string, CritterImage> imagesFromStation in critters.Images.GroupBy(image => bySite ? image.Station.ToSite() : image.Station))
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
                    foreach (CritterImage image in imagesOfSpecies.OrderBy(image => image.DateTime))
                    {
                        CritterDetection detection = new CritterDetection(image);
                        if (bySite)
                        {
                            detection.Station = imagesFromStation.Key;
                        }

                        // assume camera clock is in the same time zone as this computer
                        if (detection.StartDateTime.Kind == DateTimeKind.Local)
                        {
                            detection.StartDateTime = detection.StartDateTime.ToUniversalTime();
                        }
                        if (detection.EndDateTime.Kind == DateTimeKind.Local)
                        {
                            detection.EndDateTime = detection.StartDateTime.ToUniversalTime();
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

        public bool TryReadCsv(string filePath, out List<string> importErrors)
        {
            importErrors = new List<string>();

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader csvReader = new StreamReader(stream))
                {
                    // validate .csv file headers against the expectations
                    // Date and Time columns in the .csv are redundant with the DateTime and UtcOffset columns and are ignored
                    List<string> dataLabelsFromHeader = this.ReadAndParseLine(csvReader);
                    List<string> dataLabelsInCritterDetectionButNotInHeader = CritterDetections.CsvColumns.Except(dataLabelsFromHeader).ToList();
                    foreach (string dataLabel in dataLabelsInCritterDetectionButNotInHeader)
                    {
                        importErrors.Add("- A column with the header '" + dataLabel + "' is required for a critter detection but nothing matches that in the .csv file." + Environment.NewLine);
                    }
                    List<string> dataLabelsInHeaderButNotCritterDetection = dataLabelsFromHeader.Except(CritterDetections.CsvColumns).ToList();
                    foreach (string dataLabel in dataLabelsInHeaderButNotCritterDetection)
                    {
                        importErrors.Add("- A column with the header '" + dataLabel + "' is present in the .csv file but nothing matches that in the critter detection schema." + Environment.NewLine);
                    }

                    if (importErrors.Count > 0)
                    {
                        return false;
                    }

                    // read image updates from the CSV file
                    int fileIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.File);
                    int folderIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.Folder);
                    int relativePathIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.RelativePath);
                    int startDateTimeIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.StartDateTime);
                    int endDateTimeIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.EndDateTime);
                    int utcOffsetIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.UtcOffset);
                    int surveyIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.Survey);
                    int stationIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.Station);
                    int triggerSourceIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.TriggerSource);
                    int confidenceIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.Confidence);
                    int identificationIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.Identification);
                    int ageIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.Age);
                    int groupTypeIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.GroupType);
                    int activityIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.Activity);
                    int pelageIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.Pelage);
                    int commentsIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.Comments);
                    for (List<string> row = this.ReadAndParseLine(csvReader); row != null; row = this.ReadAndParseLine(csvReader))
                    {
                        if (row.Count == CritterDetections.CsvColumns.Count - 1)
                        {
                            // .csv files are ambiguous in the sense a trailing comma may or may not be present at the end of the line
                            // if the final field has a value this case isn't a concern, but if the final field has no value then there's
                            // no way for the parser to know the exact number of fields in the line
                            row.Add(String.Empty);
                        }
                        else if (row.Count != CritterDetections.CsvColumns.Count)
                        {
                            Debug.Assert(false, String.Format("Expected {0} fields in line {1} but found {2}.", CritterDetections.CsvColumns.Count, String.Join(",", row), row.Count));
                        }

                        CritterDetection detection = new CritterDetection();
                        detection.File = row[fileIndex];
                        detection.Folder = row[folderIndex];
                        detection.RelativePath = row[relativePathIndex];
                        detection.StartDateTime = this.ParseUtcDateTime(row[startDateTimeIndex]);
                        detection.EndDateTime = this.ParseUtcDateTime(row[endDateTimeIndex]);
                        detection.UtcOffset = this.ParseUtcOffset(row[utcOffsetIndex]);
                        detection.Survey = row[surveyIndex];
                        detection.Station = row[stationIndex];
                        detection.TriggerSource = this.ParseEnum<TriggerSource>(row[triggerSourceIndex]);
                        detection.Confidence = this.ParseEnum<Confidence>(row[confidenceIndex]);
                        detection.Identification = row[identificationIndex];
                        detection.Age = this.ParseEnum<Age>(row[ageIndex]);
                        detection.GroupType = this.ParseEnum<GroupType>(row[groupTypeIndex]);
                        detection.Activity = this.ParseEnum<Activity>(row[activityIndex]);
                        detection.Pelage = row[pelageIndex];
                        detection.Comments = row[commentsIndex];
                        this.Detections.Add(detection);
                    }

                    return true;
                }
            }
        }

        public void WriteCsv(string filePath)
        {
            using (TextWriter fileWriter = new StreamWriter(filePath, false))
            {
                StringBuilder header = new StringBuilder();
                foreach (string columnName in CritterDetections.CsvColumns)
                {
                    header.Append(this.AddColumnValue(columnName));
                }
                fileWriter.WriteLine(header.ToString());

                foreach (CritterDetection detection in this.Detections)
                {
                    StringBuilder row = new StringBuilder();
                    row.Append(this.AddColumnValue(detection.Station));
                    row.Append(this.AddColumnValue(detection.File));
                    row.Append(this.AddColumnValue(detection.RelativePath));
                    row.Append(this.AddColumnValue(detection.StartDateTime));
                    row.Append(this.AddColumnValue(detection.EndDateTime));
                    row.Append(this.AddColumnValue(detection.UtcOffset));
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
