using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace CritterShell.Critters
{
    public class StationData : SpreadsheetReaderWriter
    {
        private static readonly ReadOnlyCollection<ColumnDefinition> Columns;

        public List<Station> Stations { get; set; }

        static StationData()
        {
            StationData.Columns = new List<ColumnDefinition>()
            {
                new ColumnDefinition(Constant.StationColumn.ID, true),
                new ColumnDefinition(Constant.StationColumn.Name, true),
                new ColumnDefinition(Constant.StationColumn.DateSet, true),
                new ColumnDefinition(Constant.StationColumn.DateRemoved, true)
            }.AsReadOnly();
        }

        public StationData()
        {
            this.Stations = new List<Station>();
        }

        public StationData GetSites()
        {
            StationData sites = new StationData();
            Dictionary<string, List<Station>> stationsBySiteID = new Dictionary<string, List<Station>>();
            foreach (Station stationToMerge in this.Stations)
            {
                string siteID = stationToMerge.ID.ToSite();
                Station site = sites.Stations.Where(station => station.ID == siteID).FirstOrDefault();
                if (site == null)
                {
                    site = new Station(stationToMerge)
                    {
                      ID = siteID
                    };
                    sites.Stations.Add(site);
                }
                else
                {
                    site.MergeUptime(stationToMerge);
                }
            }

            return sites;
        }

        protected override FileReadResult TryRead(Func<List<string>> readLine)
        {
            // validate header row against expectations
            List<string> dataLabelsFromHeader = readLine.Invoke();
            FileReadResult readResult = this.VerifyHeader(dataLabelsFromHeader, StationData.Columns);
            if (readResult.Failed)
            {
                return readResult;
            }

            // read data
            int idIndex = dataLabelsFromHeader.IndexOf(Constant.StationColumn.ID);
            int nameIndex = dataLabelsFromHeader.IndexOf(Constant.StationColumn.Name);
            int dateSetIndex = dataLabelsFromHeader.IndexOf(Constant.StationColumn.DateSet);
            int dateRemovedIndex = dataLabelsFromHeader.IndexOf(Constant.StationColumn.DateRemoved);

            for (List<string> row = readLine.Invoke(); row != null; row = readLine.Invoke())
            {
                if (row.Count == StationData.Columns.Count - 1)
                {
                    // .csv files are ambiguous in the sense a trailing comma may or may not be present at the end of the line
                    // if the final field has a value this case isn't a concern, but if the final field has no value then there's
                    // no way for the parser to know the exact number of fields in the line
                    row.Add(String.Empty);
                }
                else if (row.Count != StationData.Columns.Count)
                {
                    Debug.Assert(false, String.Format("Expected {0} fields in line {1} but found {2}.", StationData.Columns.Count, String.Join(",", row), row.Count));
                }

                // required columns
                // Any other columns are ignored.
                if (DateTime.TryParseExact(row[dateRemovedIndex], Constant.Time.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime dateRemoved) == false)
                {
                    dateRemoved = DateTime.UtcNow;
                }
                DateTime dateSet = DateTime.ParseExact(row[dateSetIndex], Constant.Time.DateFormat, CultureInfo.InvariantCulture);
                Station station = new Station(row[idIndex], row[nameIndex], dateSet, dateRemoved);

                this.Stations.Add(station);
            }

            return readResult;
        }

        public override void WriteCsv(string filePath)
        {
            throw new NotImplementedException();
        }

        public override void WriteXlsx(string filePath, string worksheetName)
        {
            throw new NotImplementedException();
        }
    }
}
