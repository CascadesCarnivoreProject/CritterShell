using CritterShell.Critters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace CritterShell
{
    internal class ActivityObservations<TActivity> : CsvReaderWriter where TActivity : CritterActivity, new()
    {
        private static readonly ReadOnlyCollection<string> CsvColumns;

        public Dictionary<string, TActivity> ActivityByStation { get; private set; }

        static ActivityObservations()
        {
            List<string> columns = new List<string>()
            {
                Constant.DielColumn.Station,
                Constant.DielColumn.Identification
            };

            if (typeof(TActivity) == typeof(CritterDielActivity))
            {
                for (int hour = 0; hour < Constant.Time.HoursInDay; ++hour)
                {
                    columns.Add(TimeSpan.FromHours(hour).ToString(Constant.Time.HourOfDayFormatWithoutSign));
                }
            }
            else if (typeof(TActivity) == typeof(CritterMonthlyActivity))
            {
                DateTime utcNow = DateTime.UtcNow;
                for (int month = 0; month < Constant.Time.MonthsInYear; ++month)
                {
                    columns.Add(new DateTime(utcNow.Year, month + 1, 1).ToString(Constant.Time.MonthShortFormat));
                }
            }
            else
            {
                throw new NotSupportedException(String.Format("Unhandled critter activity type '{0}'.", typeof(TActivity).FullName));
            }

            columns.Add(Constant.DielColumn.Survey);

            ActivityObservations<TActivity>.CsvColumns = columns.AsReadOnly();
        }

        public ActivityObservations(CritterDetections critterDetections)
        {
            this.ActivityByStation = new Dictionary<string, TActivity>();

            foreach (CritterDetection critterDetection in critterDetections.Detections)
            {
                TActivity activity;
                if (this.ActivityByStation.TryGetValue(critterDetection.Station, out activity) == false)
                {
                    activity = new TActivity();
                    activity.Station = critterDetection.Station;
                    activity.Survey = critterDetection.Survey;
                    this.ActivityByStation.Add(critterDetection.Station, activity);
                }

                activity.Add(critterDetection);
            }
        }

        public void WriteCsv(string filePath)
        {
            using (TextWriter fileWriter = new StreamWriter(filePath, false))
            {
                StringBuilder header = new StringBuilder();
                foreach (string columnName in ActivityObservations<TActivity>.CsvColumns)
                {
                    header.Append(this.AddColumnValue(columnName));
                }
                fileWriter.WriteLine(header.ToString());

                foreach (TActivity record in this.ActivityByStation.Values.OrderBy(value => value.Station))
                {
                    foreach (string identification in record.DetectionsByIdentification.Keys.OrderBy(value => value))
                    {
                        List<double> probability = record.GetProbability(identification);

                        StringBuilder row = new StringBuilder();
                        row.Append(this.AddColumnValue(record.Station));
                        row.Append(this.AddColumnValue(identification));
                        for (int index = 0; index < probability.Count; ++index)
                        {
                            row.Append(this.AddColumnValue(probability[index]));
                        }
                        row.Append(this.AddColumnValue(record.Survey));

                        fileWriter.WriteLine(row.ToString());
                    }
                }
            }
        }
    }
}
