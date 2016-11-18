using CritterShell.Critters;
using CritterShell.Gpx;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace CritterShell
{
    internal class CritterSigns : SpreadsheetReaderWriter
    {
        private static readonly ReadOnlyCollection<ColumnDefinition> Columns;

        public List<CritterSign> Signs { get; private set; }

        static CritterSigns()
        {
            CritterSigns.Columns = new List<ColumnDefinition>()
            {
                new ColumnDefinition(Constant.CritterSignColumn.Name, true),
                new ColumnDefinition(Constant.CritterSignColumn.Latitude, true),
                new ColumnDefinition(Constant.CritterSignColumn.Longitude, true),
                new ColumnDefinition(Constant.CritterSignColumn.Elevation, true),
                new ColumnDefinition(Constant.CritterSignColumn.Time, true),
                new ColumnDefinition(Constant.CritterSignColumn.Identification, true),
                new ColumnDefinition(Constant.CritterSignColumn.Type, true)
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

        protected override FileReadResult TryRead(Func<List<string>> readLine)
        {
            throw new NotImplementedException();
        }

        public override void WriteCsv(string filePath)
        {
            using (TextWriter fileWriter = new StreamWriter(filePath, false))
            {
                StringBuilder header = new StringBuilder();
                foreach (ColumnDefinition column in CritterSigns.Columns)
                {
                    header.Append(this.AddCsvValue(column.Name));
                }
                fileWriter.WriteLine(header.ToString());

                foreach (CritterSign sign in this.Signs)
                {
                    StringBuilder row = new StringBuilder();
                    row.Append(this.AddCsvValue(sign.Name));
                    row.Append(this.AddCsvValue(sign.Latitude));
                    row.Append(this.AddCsvValue(sign.Longitude));
                    row.Append(this.AddCsvValue(sign.Elevation));
                    row.Append(this.AddCsvValue(sign.Time));
                    row.Append(this.AddCsvValue(sign.Identification));
                    row.Append(this.AddCsvValue(sign.Type.ToString().ToLowerInvariant()));
                    fileWriter.WriteLine(row.ToString());
                }
            }
        }

        public override void WriteXlsx(string filePath, string worksheetName)
        {
            using (ExcelPackage xlsxFile = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet worksheet = this.GetOrCreateBlankWorksheet(xlsxFile, worksheetName, CritterSigns.Columns);
                for (int index = 0; index < this.Signs.Count; ++index)
                {
                    CritterSign sign = this.Signs[index];
                    int row = index + 2;
                    worksheet.Cells[row, 1].Value = sign.Name;
                    worksheet.Cells[row, 2].Value = sign.Latitude;
                    worksheet.Cells[row, 3].Value = sign.Longitude;
                    worksheet.Cells[row, 4].Value = sign.Elevation;
                    worksheet.Cells[row, 5].Value = sign.Time;
                    worksheet.Cells[row, 6].Value = sign.Identification;
                    worksheet.Cells[row, 7].Value = sign.Type.ToString().ToLowerInvariant();
                }

                worksheet.Cells[1, 1, worksheet.Dimension.Rows, worksheet.Dimension.Columns].AutoFitColumns(Constant.Excel.MinimumColumnWidth, Constant.Excel.MaximumColumnWidth);
                xlsxFile.Save();
            }
        }
    }
}
