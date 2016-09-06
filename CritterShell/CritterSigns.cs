﻿using CritterShell.Critters;
using CritterShell.Gpx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace CritterShell
{
    internal class CritterSigns : CsvReaderWriter
    {
        private static readonly ReadOnlyCollection<string> CsvColumns;

        public List<CritterSign> Signs { get; private set; }

        static CritterSigns()
        {
            CritterSigns.CsvColumns = new List<string>()
            {
                Constant.CritterSignColumn.Name,
                Constant.CritterSignColumn.Latitude,
                Constant.CritterSignColumn.Latitude,
                Constant.CritterSignColumn.Elevation,
                Constant.CritterSignColumn.Time,
                Constant.CritterSignColumn.Identification,
                Constant.CritterSignColumn.Type
            }.AsReadOnly();
        }

        public CritterSigns(GpxFile gpx)
        {
            this.Signs = new List<CritterSign>();

            foreach (Waypoint waypoint in gpx.Waypoints)
            {
                CritterSign sign = new CritterSign(waypoint);
                this.Signs.Add(sign);
            }
        }

        public void WriteCsv(string filePath)
        {
            using (TextWriter fileWriter = new StreamWriter(filePath, false))
            {
                StringBuilder header = new StringBuilder();
                foreach (string columnName in CritterSigns.CsvColumns)
                {
                    header.Append(this.AddColumnValue(columnName));
                }
                fileWriter.WriteLine(header.ToString());

                foreach (CritterSign sign in this.Signs)
                {
                    StringBuilder row = new StringBuilder();
                    row.Append(this.AddColumnValue(sign.Name));
                    row.Append(this.AddColumnValue(sign.Latitude));
                    row.Append(this.AddColumnValue(sign.Latitude));
                    row.Append(this.AddColumnValue(sign.Elevation));
                    row.Append(this.AddColumnValue(sign.Time));
                    row.Append(this.AddColumnValue(sign.Identification));
                    row.Append(this.AddColumnValue(sign.Type.ToString().ToLowerInvariant()));
                    fileWriter.WriteLine(row.ToString());
                }
            }
        }
    }
}
