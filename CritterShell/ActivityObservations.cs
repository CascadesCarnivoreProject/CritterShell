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

        private Dictionary<string, List<TActivity>> groupsByStationName;
        private Dictionary<string, Station> stationsByID;

        public Dictionary<string, TActivity> ActivityByStationName { get; private set; }
        public Dictionary<string, TActivity> ActivityGroupByName { get; private set; }
        public TActivity ActivityTotal { get; private set; }
        public bool WriteProbabilities { get; set; }

        static ActivityObservations()
        {
            List<ColumnDefinition> columns = new List<ColumnDefinition>()
            {
                new ColumnDefinition(Constant.ActivityColumn.StationName, true),
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
                for (int month = 0; month < Constant.Time.MonthsInYear; ++month)
                {
                    columns.Add(new ColumnDefinition(new DateTime(utcNow.Year, month + 1, 1).ToString(Constant.Time.MonthShortFormat) + " per day", true));
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

        public ActivityObservations(CritterDetections critterDetections, StationData stations, Dictionary<string, List<string>> groups)
        {
            this.ActivityByStationName = new Dictionary<string, TActivity>();
            this.ActivityGroupByName = new Dictionary<string, TActivity>();
            this.ActivityTotal = new TActivity()
            {
                Station = new Station("total", "total", stations.Stations.Select(station => station.DateSet).Min(), stations.Stations.Select(station => station.DateRemoved).Max())
            };
            this.groupsByStationName = new Dictionary<string, List<TActivity>>();
            this.stationsByID = new Dictionary<string, Station>(stations.Stations.Count);
            this.WriteProbabilities = false;

            if (groups != null)
            {
                foreach (KeyValuePair<string, List<string>> groupDefinition in groups)
                {
                    List<Station> stationsInGroup = stations.Stations.Where(station => groupDefinition.Value.Contains(station.ID)).ToList();
                    DateTime firstStationInGroupSet = stationsInGroup.Select(station => station.DateSet).Min();
                    DateTime lastStationInGroupRemoved = stationsInGroup.Select(station => station.DateRemoved).Max();

                    TActivity group = new TActivity()
                    {
                        Station = new Station(groupDefinition.Key, groupDefinition.Key, firstStationInGroupSet, lastStationInGroupRemoved)
                    };
                    this.ActivityGroupByName.Add(groupDefinition.Key, group);

                    foreach (string siteIDPrefix in groupDefinition.Value)
                    {
                        if (this.groupsByStationName.TryGetValue(siteIDPrefix, out List<TActivity> groupsForStation) == false)
                        {
                            groupsForStation = new List<TActivity>();
                            this.groupsByStationName.Add(siteIDPrefix, groupsForStation);
                        }
                        groupsForStation.Add(group);
                    }
                }
            }

            foreach (Station station in stations.Stations)
            {
                this.stationsByID.Add(station.ID, station);
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
                if (this.ActivityByStationName.TryGetValue(critterDetection.Station, out TActivity activity) == false)
                {
                    if (this.stationsByID.TryGetValue(critterDetection.Station, out Station station) == false)
                    {
                        throw new KeyNotFoundException(String.Format("Station or site '{0}' for critter detection not found.  Are per site detections being used with station data or per station detections with site data?", critterDetection.Station));
                    }

                    activity = new TActivity()
                    {
                        Station = station
                    };
                    activity.Surveys.Add(critterDetection.Survey);
                    this.ActivityByStationName.Add(critterDetection.Station, activity);
                }
                activity.Add(critterDetection);

                if (this.groupsByStationName.TryGetValue(critterDetection.Station, out List<TActivity> groupsForStation))
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

        private void Write(Action<TActivity, string, List<double>, double> writeRow)
        {
            List<TActivity> activity = this.ActivityByStationName.Values.OrderBy(value => value.Station.Name).ToList();
            activity.AddRange(this.ActivityGroupByName.Values.OrderBy(value => value.Station.Name));
            activity.Add(this.ActivityTotal);

            foreach (TActivity record in activity)
            {
                foreach (string identification in record.DetectionsByIdentification.Keys.OrderBy(value => value))
                {
                    double detectionCount;
                    List<double> observations;
                    if (this.WriteProbabilities)
                    {
                        observations = record.GetProbability(identification, out detectionCount);
                    }
                    else
                    {
                        observations = record.GetActivity(identification, out detectionCount);
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

                this.Write((TActivity record, string identification, List<double> observations, double detectionCount) =>
                {
                    StringBuilder row = new StringBuilder();
                    row.Append(this.AddCsvValue(record.Station.Name));
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
                this.Write((TActivity record, string identification, List<double> observations, double detectionCount) =>
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
