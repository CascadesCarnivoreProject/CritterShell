using CritterShell.Critters;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace CritterShell
{
    internal class CritterDetections : SpreadsheetReaderWriter
    {
        private static readonly ReadOnlyCollection<ColumnDefinition> Columns;

        public List<CritterDetection> Detections { get; private set; }

        static CritterDetections()
        {
            CritterDetections.Columns = new List<ColumnDefinition>()
            {
                new ColumnDefinition(Constant.DetectionColumn.Station, true),
                new ColumnDefinition(Constant.DetectionColumn.File, true),
                new ColumnDefinition(Constant.DetectionColumn.RelativePath, true),
                new ColumnDefinition(Constant.DetectionColumn.StartDateTime, true),
                new ColumnDefinition(Constant.DetectionColumn.EndDateTime, true),
                new ColumnDefinition(Constant.DetectionColumn.UtcOffset, true),
                new ColumnDefinition(Constant.DetectionColumn.Duration),
                new ColumnDefinition(Constant.DetectionColumn.TriggerSource),
                new ColumnDefinition(Constant.DetectionColumn.Identification, true),
                new ColumnDefinition(Constant.DetectionColumn.Confidence),
                new ColumnDefinition(Constant.DetectionColumn.GroupType),
                new ColumnDefinition(Constant.DetectionColumn.Age),
                new ColumnDefinition(Constant.DetectionColumn.Pelage),
                new ColumnDefinition(Constant.DetectionColumn.Activity),
                new ColumnDefinition(Constant.DetectionColumn.Comments),
                new ColumnDefinition(Constant.DetectionColumn.Survey)
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

        protected override FileReadResult TryRead(Func<List<string>> readLine)
        {
            // validate header row against expectations
            List<string> dataLabelsFromHeader = readLine.Invoke();
            FileReadResult readResult = this.VerifyHeader(dataLabelsFromHeader, CritterDetections.Columns);
            if (readResult.Failed)
            {
                return readResult;
            }

            // read data
            int fileIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.File);
            int relativePathIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.RelativePath);
            int startDateTimeIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.StartDateTime);
            int endDateTimeIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.EndDateTime);
            int utcOffsetIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.UtcOffset);
            int stationIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.Station);
            int identificationIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.Identification);

            int activityIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.Activity);
            int ageIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.Age);
            int commentsIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.Comments);
            int confidenceIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.Confidence);
            int groupTypeIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.GroupType);
            int pelageIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.Pelage);
            int triggerSourceIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.TriggerSource);
            int surveyIndex = dataLabelsFromHeader.IndexOf(Constant.DetectionColumn.Survey);
            for (List<string> row = readLine.Invoke(); row != null; row = readLine.Invoke())
            {
                if (row.Count == CritterDetections.Columns.Count - 1)
                {
                    // .csv files are ambiguous in the sense a trailing comma may or may not be present at the end of the line
                    // if the final field has a value this case isn't a concern, but if the final field has no value then there's
                    // no way for the parser to know the exact number of fields in the line
                    row.Add(String.Empty);
                }
                else if (row.Count != CritterDetections.Columns.Count)
                {
                    Debug.Assert(false, String.Format("Expected {0} fields in line {1} but found {2}.", CritterDetections.Columns.Count, String.Join(",", row), row.Count));
                }

                CritterDetection detection = new CritterDetection();
                // required columns
                detection.File = row[fileIndex];
                detection.RelativePath = row[relativePathIndex];
                detection.StartDateTime = this.ParseUtcDateTime(row[startDateTimeIndex]);
                detection.EndDateTime = this.ParseUtcDateTime(row[endDateTimeIndex]);
                detection.UtcOffset = this.ParseUtcOffset(row[utcOffsetIndex]);
                detection.Station = row[stationIndex];
                detection.Identification = row[identificationIndex];

                // optional columns
                if (activityIndex != -1)
                {
                    detection.Activity = this.ParseEnum<Activity>(row[activityIndex]);
                }
                if (ageIndex != -1)
                {
                    detection.Age = this.ParseEnum<Age>(row[ageIndex]);
                }
                if (commentsIndex != -1)
                {
                    detection.Comments = row[commentsIndex];
                }
                if (confidenceIndex != -1)
                {
                    detection.Confidence = this.ParseEnum<Confidence>(row[confidenceIndex]);
                }
                if (groupTypeIndex != -1)
                {
                    detection.GroupType = this.ParseEnum<GroupType>(row[groupTypeIndex]);
                }
                if (pelageIndex != -1)
                {
                    detection.Pelage = row[pelageIndex];
                }
                if (triggerSourceIndex != -1)
                {
                    detection.TriggerSource = this.ParseEnum<TriggerSource>(row[triggerSourceIndex]);
                }
                if (surveyIndex != -1)
                {
                    detection.Survey = row[surveyIndex];
                }
                this.Detections.Add(detection);
            }

            return readResult;
        }

        public override void WriteCsv(string filePath)
        {
            using (TextWriter fileWriter = new StreamWriter(filePath, false))
            {
                StringBuilder header = new StringBuilder();
                foreach (ColumnDefinition column in CritterDetections.Columns)
                {
                    header.Append(this.AddCsvValue(column.Name));
                }
                fileWriter.WriteLine(header.ToString());

                foreach (CritterDetection detection in this.Detections)
                {
                    StringBuilder row = new StringBuilder();
                    row.Append(this.AddCsvValue(detection.Station));
                    row.Append(this.AddCsvValue(detection.File));
                    row.Append(this.AddCsvValue(detection.RelativePath));
                    row.Append(this.AddCsvValue(detection.StartDateTime));
                    row.Append(this.AddCsvValue(detection.EndDateTime));
                    row.Append(this.AddCsvValue(detection.UtcOffset));
                    row.Append(this.AddCsvValue(detection.Duration.ToString().ToLowerInvariant()));
                    row.Append(this.AddCsvValue(detection.TriggerSource.ToString().ToLowerInvariant()));
                    row.Append(this.AddCsvValue(detection.Identification));
                    row.Append(this.AddCsvValue(detection.Confidence.ToString().ToLowerInvariant()));
                    row.Append(this.AddCsvValue(detection.GroupType.ToString().ToLowerInvariant()));
                    row.Append(this.AddCsvValue(detection.Age.ToString().ToLowerInvariant()));
                    row.Append(this.AddCsvValue(detection.Pelage));
                    row.Append(this.AddCsvValue(detection.Activity.ToString().ToLowerInvariant()));
                    row.Append(this.AddCsvValue(detection.Comments));
                    row.Append(this.AddCsvValue(detection.Survey));
                    fileWriter.WriteLine(row.ToString());
                }
            }
        }

        public override void WriteXlsx(string filePath, string worksheetName)
        {
            using (ExcelPackage xlsxFile = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet worksheet = this.GetOrCreateBlankWorksheet(xlsxFile, worksheetName, CritterDetections.Columns);
                for (int index = 0; index < this.Detections.Count; ++index)
                {
                    CritterDetection detection = this.Detections[index];
                    int row = index + 2;
                    worksheet.Cells[row, 1].Value = detection.Station;
                    worksheet.Cells[row, 2].Value = detection.File;
                    worksheet.Cells[row, 3].Value = detection.RelativePath;
                    worksheet.Cells[row, 4].Value = detection.StartDateTime.ToString(Constant.Time.UtcDateTimeFormat);
                    worksheet.Cells[row, 5].Value = detection.EndDateTime.ToString(Constant.Time.UtcDateTimeFormat);
                    worksheet.Cells[row, 6].Value = detection.UtcOffset.TotalHours.ToString(Constant.Time.UtcOffsetFormat);
                    worksheet.Cells[row, 7].Value = detection.Duration.ToString().ToLowerInvariant();
                    worksheet.Cells[row, 8].Value = detection.TriggerSource.ToString().ToLowerInvariant();
                    worksheet.Cells[row, 9].Value = detection.Identification;
                    worksheet.Cells[row, 10].Value = detection.Confidence.ToString().ToLowerInvariant();
                    worksheet.Cells[row, 11].Value = detection.GroupType.ToString().ToLowerInvariant();
                    worksheet.Cells[row, 12].Value = detection.Age.ToString().ToLowerInvariant();
                    worksheet.Cells[row, 13].Value = detection.Pelage;
                    worksheet.Cells[row, 14].Value = detection.Activity.ToString().ToLowerInvariant();
                    worksheet.Cells[row, 15].Value = detection.Comments;
                    worksheet.Cells[row, 16].Value = detection.Survey;
                }

                worksheet.Cells[1, 1, worksheet.Dimension.Rows, worksheet.Dimension.Columns].AutoFitColumns(Constant.Excel.MinimumColumnWidth, Constant.Excel.MaximumColumnWidth);
                xlsxFile.Save();
            }
        }
    }
}