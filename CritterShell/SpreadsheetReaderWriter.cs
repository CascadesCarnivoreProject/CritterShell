using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CritterShell
{
    /// <summary>
    /// Base functionality for reading and writing .csv and .xlsx files.
    /// </summary>
    public abstract class SpreadsheetReaderWriter
    {
        protected string AddCsvValue(DateTime value)
        {
            return this.AddCsvValue(value.ToString(Constant.Time.UtcDateTimeFormat));
        }

        protected string AddCsvValue(double value)
        {
            return this.AddCsvValue(value.ToString());
        }

        protected string AddCsvValue(TimeSpan utcOffset)
        {
            return this.AddCsvValue(utcOffset.TotalHours.ToString(Constant.Time.UtcOffsetFormat));
        }

        // Check if there is any Quotation Mark '"', a Comma ',', a Line Feed \x0A,  or Carriage Return \x0D
        // and escape it as needed
        protected string AddCsvValue(string value)
        {
            if (value == null)
            {
                return ",";
            }
            if (value.IndexOfAny("\",\x0A\x0D".ToCharArray()) > -1)
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"" + ",";
            }
            else
            {
                return value + ",";
            }
        }

        protected ExcelWorksheet GetOrCreateBlankWorksheet(ExcelPackage xlsxFile, string worksheetName, ReadOnlyCollection<string> columnHeaders)
        {
            // get empty worksheet
            ExcelWorksheet worksheet = xlsxFile.Workbook.Worksheets.FirstOrDefault(sheet => sheet.Name == worksheetName);
            if (worksheet == null)
            {
                worksheet = xlsxFile.Workbook.Worksheets.Add(worksheetName);
                worksheet.View.FreezePanes(2, 1);
            }
            else
            {
                worksheet.Cells.Clear();
            }

            // write header
            for (int index = 0; index < columnHeaders.Count; ++index)
            {
                worksheet.Cells[1, index + 1].Value = columnHeaders[index];
            }

            ExcelRange headerCells = worksheet.Cells[1, 1, 1, columnHeaders.Count];
            headerCells.AutoFilter = true;
            headerCells.Style.Font.Bold = true;

            return worksheet;
        }

        protected bool ParseBoolean(string value)
        {
            if (String.IsNullOrWhiteSpace(value) || value == "0")
            {
                return false;
            }
            if (value == "1")
            {
                return true;
            }
            return Boolean.Parse(value);
        }

        protected TEnum ParseEnum<TEnum>(string value) where TEnum : struct, IComparable, IConvertible, IFormattable
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                return default(TEnum);
            }
            return (TEnum)Enum.Parse(typeof(TEnum), value, true);
        }

        protected DateTime ParseUtcDateTime(string value)
        {
            return DateTime.ParseExact(value, Constant.Time.UtcDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
        }

        protected TimeSpan ParseUtcOffset(string value)
        {
            return TimeSpan.FromHours(double.Parse(value));
        }

        protected List<string> ReadAndParseCsvLine(StreamReader csvReader)
        {
            string unparsedLine = csvReader.ReadLine();
            if (unparsedLine == null)
            {
                return null;
            }

            List<string> parsedLine = new List<string>();
            bool isFieldEscaped = false;
            int fieldStart = 0;
            bool inField = false;
            for (int index = 0; index < unparsedLine.Length; ++index)
            {
                char currentCharacter = unparsedLine[index];
                if (inField == false)
                {
                    if (currentCharacter == '\"')
                    {
                        // start of escaped field
                        isFieldEscaped = true;
                        fieldStart = index + 1;
                    }
                    else if (currentCharacter == ',')
                    {
                        // empty field
                        // promote null values to empty values to prevent the presence of SQNull objects in data tables
                        // much Timelapse code assumes data table fields can be blindly cast to string and breaks once the data table has been
                        // refreshed after null values are inserted
                        parsedLine.Add(String.Empty);
                        continue;
                    }
                    else
                    {
                        // start of unescaped field
                        fieldStart = index;
                    }

                    inField = true;
                }
                else
                {
                    if (currentCharacter == ',' && isFieldEscaped == false)
                    {
                        // end of unescaped field
                        inField = false;
                        string field = unparsedLine.Substring(fieldStart, index - fieldStart);
                        parsedLine.Add(field);
                    }
                    else if (currentCharacter == '\"' && isFieldEscaped)
                    {
                        // escaped character encountered; check for end of escaped field
                        int nextIndex = index + 1;
                        if (nextIndex < unparsedLine.Length && unparsedLine[nextIndex] == ',')
                        {
                            // end of escaped field
                            inField = false;
                            isFieldEscaped = false;
                            string field = unparsedLine.Substring(fieldStart, index - fieldStart);
                            parsedLine.Add(field);
                            ++index;
                        }
                    }
                }
            }

            // if the last character is a non-comma add the final (non-empty) field
            // final empty fields are ambiguous at this level and therefore handled by the caller
            if (inField)
            {
                string field = unparsedLine.Substring(fieldStart, unparsedLine.Length - fieldStart);
                parsedLine.Add(field);
            }

            return parsedLine;
        }

        protected List<string> ReadXlsxRow(ExcelWorksheet worksheet, int row)
        {
            if (worksheet.Dimension.Rows < row)
            {
                return null;
            }

            List<string> rowContent = new List<string>(worksheet.Dimension.Columns);
            for (int column = 1; column <= worksheet.Dimension.Columns; ++column)
            {
                rowContent.Add(worksheet.Cells[row, column].Text);
            }
            return rowContent;
        }

        public bool TryRead(string filePath, string worksheetName, out List<string> importErrors)
        {
            if (String.Equals(Path.GetExtension(filePath), Constant.Csv.Extension, StringComparison.OrdinalIgnoreCase))
            {
                return this.TryReadCsv(filePath, out importErrors);
            }
            else if (String.Equals(Path.GetExtension(filePath), Constant.Excel.Extension, StringComparison.OrdinalIgnoreCase))
            {
                return this.TryReadXlsx(filePath, worksheetName, out importErrors);
            }
            else
            {
                throw new NotSupportedException(String.Format("Unknown output file type {0}.", Path.GetExtension(filePath)));
            }
        }

        protected abstract bool TryRead(Func<List<string>> readLine, out List<string> importErrors);

        private bool TryReadCsv(string filePath, out List<string> importErrors)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader csvReader = new StreamReader(stream))
                {
                    return this.TryRead(() =>
                    {
                        return this.ReadAndParseCsvLine(csvReader);
                    },
                        out importErrors);
                }
            }
        }

        private bool TryReadXlsx(string filePath, string worksheetName, out List<string> importErrors)
        {
            if (String.IsNullOrWhiteSpace(worksheetName))
            {
                throw new ArgumentOutOfRangeException("worksheetName");
            }

            using (ExcelPackage xlsxFile = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet worksheet = xlsxFile.Workbook.Worksheets.First(sheet => sheet.Name == worksheetName);
                int row = 0;
                return this.TryRead(() =>
                {
                    return this.ReadXlsxRow(worksheet, ++row);
                },
                    out importErrors);
            }
        }

        protected bool VerifyHeader(List<string> columnsFromFile, ReadOnlyCollection<string> expectedColumns, out List<string> importErrors)
        {
            importErrors = new List<string>();
            if (columnsFromFile == null)
            {
                return false;
            }

            List<string> columnsMissingFromFile = expectedColumns.Except(columnsFromFile).ToList();
            foreach (string dataLabel in columnsMissingFromFile)
            {
                importErrors.Add("- The column '" + dataLabel + "' is not present in the input file." + Environment.NewLine);
            }
            List<string> extraColumnsInFile = columnsFromFile.Except(expectedColumns).ToList();
            foreach (string dataLabel in extraColumnsInFile)
            {
                importErrors.Add("- The column '" + dataLabel + "' is unexpectedly present in the input file." + Environment.NewLine);
            }

            return importErrors.Count == 0;
        }

        public abstract void WriteCsv(string filePath);
        public abstract void WriteXlsx(string filePath, string worksheetName);
    }
}
