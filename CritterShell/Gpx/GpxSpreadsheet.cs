using CritterShell.Critters;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace CritterShell.Gpx
{
    internal class GpxSpreadsheet : SpreadsheetReaderWriter
    {
        private static readonly ReadOnlyCollection<ColumnDefinition> Columns;

        private readonly GpxFile file;

        static GpxSpreadsheet()
        {
            GpxSpreadsheet.Columns = new List<ColumnDefinition>()
            {
                new ColumnDefinition(Constant.GpxColumn.Name, true),
                new ColumnDefinition(Constant.GpxColumn.Latitude, true),
                new ColumnDefinition(Constant.GpxColumn.Longitude, true),
                new ColumnDefinition(Constant.GpxColumn.Elevation, true),
                new ColumnDefinition(Constant.GpxColumn.Time, true),
                new ColumnDefinition(Constant.GpxColumn.Comment, true),
                new ColumnDefinition(Constant.GpxColumn.Description, true),
                new ColumnDefinition(Constant.GpxColumn.Categories, true)
            }.AsReadOnly();
        }

        public GpxSpreadsheet(GpxFile file)
        {
            this.file = file;
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
                foreach (ColumnDefinition column in GpxSpreadsheet.Columns)
                {
                    header.Append(this.AddCsvValue(column.Name));
                }
                fileWriter.WriteLine(header.ToString());

                foreach (Waypoint waypoint in this.file.Waypoints)
                {
                    StringBuilder row = new StringBuilder();
                    row.Append(this.AddCsvValue(waypoint.Name));
                    row.Append(this.AddCsvValue(waypoint.Latitude));
                    row.Append(this.AddCsvValue(waypoint.Longitude));
                    row.Append(this.AddCsvValue(waypoint.Elevation));
                    row.Append(this.AddCsvValue(waypoint.Time));
                    row.Append(this.AddCsvValue(waypoint.Comment));
                    row.Append(this.AddCsvValue(waypoint.Description));
                    fileWriter.WriteLine(row.ToString());
                }
            }
        }

        public override void WriteXlsx(string filePath, string worksheetName)
        {
            using (ExcelPackage xlsxFile = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet worksheet = this.GetOrCreateBlankWorksheet(xlsxFile, worksheetName, GpxSpreadsheet.Columns);
                for (int index = 0; index < this.file.Waypoints.Count; ++index)
                {
                    Waypoint waypoint = this.file.Waypoints[index];
                    int row = index + 2;
                    worksheet.Cells[row, 1].Value = this.MaybeConvertToIntegerForExcel(waypoint.Name);
                    worksheet.Cells[row, 2].Value = waypoint.Latitude;
                    worksheet.Cells[row, 3].Value = waypoint.Longitude;
                    worksheet.Cells[row, 4].Value = waypoint.Elevation;
                    worksheet.Cells[row, 5].Value = waypoint.Time.ToString(Constant.Time.UtcDateTimeFormat);
                    worksheet.Cells[row, 6].Value = this.MaybeConvertToIntegerForExcel(waypoint.Comment);
                    worksheet.Cells[row, 7].Value = this.MaybeConvertToIntegerForExcel(waypoint.Description);

                    if (waypoint.Extensions != null)
                    {
                        worksheet.Cells[row, 8].Value = String.Join("|", waypoint.Extensions.Categories);
                    }
                }

                worksheet.Cells[1, 1, worksheet.Dimension.Rows, worksheet.Dimension.Columns].AutoFitColumns(Constant.Excel.MinimumColumnWidth, Constant.Excel.MaximumColumnWidth);
                xlsxFile.Save();
            }
        }
    }
}
