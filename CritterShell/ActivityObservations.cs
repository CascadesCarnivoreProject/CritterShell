using CritterShell.Critters;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace CritterShell
{
    internal class ActivityObservations<TActivity> : SpreadsheetReaderWriter where TActivity : CritterActivity, new()
    {
        private static readonly ReadOnlyCollection<ColumnDefinition> Columns;

        private Dictionary<string, List<TActivity>> groupsByStation;

        public Dictionary<string, TActivity> ActivityByStation { get; private set; }
        public Dictionary<string, TActivity> ActivityGroupByName { get; private set; }
        public TActivity ActivityTotal { get; private set; }
        public bool WriteProbabilities { get; set; }
        public bool WriteTotal { get; set; }

        static ActivityObservations()
        {
            List<ColumnDefinition> columns = new List<ColumnDefinition>()
            {
                new ColumnDefinition(Constant.ActivityColumn.Station, true),
                new ColumnDefinition(Constant.ActivityColumn.Identification, true)
            };

            if (typeof(TActivity) == typeof(CritterDielActivity))
            {
                for (int hour = 0; hour < Constant.Time.HoursInDay; ++hour)
                {
                    TimeSpan midpointOfHour = TimeSpan.FromHours(hour) + TimeSpan.FromMinutes(30.0);
                    columns.Add(new ColumnDefinition(midpointOfHour.ToString(Constant.Time.HourOfDayFormatWithoutSign), true));
                }
            }
            else if (typeof(TActivity) == typeof(CritterMonthlyActivity))
            {
                DateTime utcNow = DateTime.UtcNow;
                for (int month = 0; month < Constant.Time.MonthsInYear; ++month)
                {
                    columns.Add(new ColumnDefinition(new DateTime(utcNow.Year, month + 1, 1).ToString(Constant.Time.MonthShortFormat), true));
                }
            }
            else
            {
                throw new NotSupportedException(String.Format("Unhandled critter activity type '{0}'.", typeof(TActivity).FullName));
            }

            columns.Add(new ColumnDefinition(Constant.ActivityColumn.N, true));
            columns.Add(new ColumnDefinition(Constant.ActivityColumn.Survey));

            ActivityObservations<TActivity>.Columns = columns.AsReadOnly();
        }

        public ActivityObservations(CritterDetections critterDetections, Dictionary<string, List<string>> groups)
        {
            this.ActivityByStation = new Dictionary<string, TActivity>();
            this.ActivityGroupByName = new Dictionary<string, TActivity>();
            this.ActivityTotal = new TActivity();
            this.ActivityTotal.Station = "total";
            this.groupsByStation = new Dictionary<string, List<TActivity>>();
            this.WriteProbabilities = false;
            this.WriteTotal = false;

            if (groups != null)
            {
                foreach (KeyValuePair<string, List<string>> groupDefinition in groups)
                {
                    TActivity group = new TActivity();
                    group.Station = groupDefinition.Key;
                    this.ActivityGroupByName.Add(groupDefinition.Key, group);

                    foreach (string station in groupDefinition.Value)
                    {
                        List<TActivity> groupsForStation;
                        if (this.groupsByStation.TryGetValue(station, out groupsForStation) == false)
                        {
                            groupsForStation = new List<TActivity>();
                            this.groupsByStation.Add(station, groupsForStation);
                        }
                        groupsForStation.Add(group);
                    }
                }
            }

            if (critterDetections.Detections.Count < 1)
            {
                return;
            }

            this.ActivityTotal.Surveys.Add(critterDetections.Detections[0].Survey);
            foreach (TActivity group in this.ActivityGroupByName.Values)
            {
                group.Surveys.Add(critterDetections.Detections[0].Survey);
            }

            foreach (CritterDetection critterDetection in critterDetections.Detections)
            {
                TActivity activity;
                if (this.ActivityByStation.TryGetValue(critterDetection.Station, out activity) == false)
                {
                    activity = new TActivity();
                    activity.Station = critterDetection.Station;
                    activity.Surveys.Add(critterDetection.Survey);
                    this.ActivityByStation.Add(critterDetection.Station, activity);
                }
                activity.Add(critterDetection);

                List<TActivity> groupsForStation;
                if (this.groupsByStation.TryGetValue(critterDetection.Station, out groupsForStation))
                {
                    foreach (TActivity group in groupsForStation)
                    {
                        group.Add(critterDetection);
                    }
                }

                this.ActivityTotal.Add(critterDetection);
            }
        }

        protected override FileReadResult TryRead(Func<List<string>> readLine)
        {
            throw new NotImplementedException();
        }

        private void Write(Action<TActivity, string, List<double>, int> writeRow)
        {
            IEnumerable<TActivity> activity = this.ActivityByStation.Values.OrderBy(value => value.Station);
            activity = activity.Union(this.ActivityGroupByName.Values.OrderBy(value => value.Station));
            if (this.WriteTotal)
            {
                activity = activity.Union(new List<TActivity>() { this.ActivityTotal });
            }

            foreach (TActivity record in activity)
            {
                foreach (string identification in record.DetectionsByIdentification.Keys.OrderBy(value => value))
                {
                    int detectionCount;
                    List<double> observations;
                    if (this.WriteProbabilities)
                    {
                        observations = record.GetProbability(identification, out detectionCount);
                    }
                    else
                    {
                        List<int> detections = record.DetectionsByIdentification[identification];
                        detectionCount = detections.Sum();
                        observations = detections.Select(value => (double)value).ToList();
                    }

                    writeRow.Invoke(record, identification, observations, detectionCount);
                }
            }
        }

        public override void WriteCsv(string filePath)
        {
            using (TextWriter fileWriter = new StreamWriter(filePath, false))
            {
                StringBuilder header = new StringBuilder();
                foreach (ColumnDefinition column in ActivityObservations<TActivity>.Columns)
                {
                    header.Append(this.AddCsvValue(column.Name));
                }
                fileWriter.WriteLine(header.ToString());

                this.Write((TActivity record, string identification, List<double> observations, int detectionCount) =>
                {
                    StringBuilder row = new StringBuilder();
                    row.Append(this.AddCsvValue(record.Station));
                    row.Append(this.AddCsvValue(identification));
                    for (int index = 0; index < observations.Count; ++index)
                    {
                        row.Append(this.AddCsvValue(observations[index]));
                    }
                    row.Append(this.AddCsvValue(detectionCount));
                    row.Append(this.AddCsvValue(String.Join("|", record.Surveys)));

                    fileWriter.WriteLine(row.ToString());
                });
            }
        }

        public override void WriteXlsx(string filePath, string worksheetName)
        {
            using (ExcelPackage xlsxFile = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet worksheet = this.GetOrCreateBlankWorksheet(xlsxFile, worksheetName, ActivityObservations<TActivity>.Columns);
                int row = 1;
                this.Write((TActivity record, string identification, List<double> observations, int detectionCount) =>
                {
                    ++row;
                    int column = 0;
                    worksheet.Cells[row, ++column].Value = record.Station;
                    worksheet.Cells[row, ++column].Value = identification;
                    for (int observation = 0; observation < observations.Count; ++observation)
                    {
                        worksheet.Cells[row, ++column].Value = observations[observation];
                    }
                    worksheet.Cells[row, ++column].Value = detectionCount;
                    worksheet.Cells[row, ++column].Value = record.Surveys;
                });

                // match column widths to content
                worksheet.Cells[1, 1, worksheet.Dimension.Rows, worksheet.Dimension.Columns].AutoFitColumns(Constant.Excel.MinimumColumnWidth, Constant.Excel.MaximumColumnWidth);

                xlsxFile.Save();
            }
        }
    }
}
