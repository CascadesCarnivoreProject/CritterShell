using CritterShell.Critters;
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

        protected ExcelWorksheet GetOrCreateBlankWorksheet(ExcelPackage xlsxFile, string worksheetName, ReadOnlyCollection<ColumnDefinition> columnHeaders)
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
                worksheet.Cells[1, index + 1].Value = columnHeaders[index].Name;
            }

            ExcelRange headerCells = worksheet.Cells[1, 1, 1, columnHeaders.Count];
            headerCells.AutoFilter = true;
            headerCells.Style.Font.Bold = true;

            return worksheet;
        }

        protected object MaybeConvertToIntegerForExcel(string valueAsString)
        {
            // as of 2017, up to date installs of Excel 2016 raise green marks on cells containing numbers formatted as strings
            // Attempt automatic conversion to save the user having to manually convert affected cells to numbers.  This has the side effect of removing any
            // leading zeros but Excel does that as well.
            if (Int32.TryParse(valueAsString, out int valueAsInteger))
            {
                return valueAsInteger;
            }
            return valueAsString;
        }

        protected bool ParseBoolean(string value)
        {
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
                ExcelRange cell = worksheet.Cells[row, column];
                if (cell.Value is bool cellValue)
                {
                    rowContent.Add(cellValue ? Boolean.TrueString : Boolean.FalseString);
                }
                else
                {
                    rowContent.Add(cell.Text);
                }
            }
            return rowContent;
        }

        public FileReadResult TryRead(string filePath, string worksheetName)
        {
            if (String.Equals(Path.GetExtension(filePath), Constant.Csv.Extension, StringComparison.OrdinalIgnoreCase))
            {
                return this.TryReadCsv(filePath);
            }
            else if (String.Equals(Path.GetExtension(filePath), Constant.Excel.Extension, StringComparison.OrdinalIgnoreCase))
            {
                return this.TryReadXlsx(filePath, worksheetName);
            }
            else
            {
                throw new NotSupportedException(String.Format("Unknown output file type {0}.", Path.GetExtension(filePath)));
            }
        }

        protected abstract FileReadResult TryRead(Func<List<string>> readLine);

        private FileReadResult TryReadCsv(string filePath)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader csvReader = new StreamReader(stream))
                {
                    return this.TryRead(() => { return this.ReadAndParseCsvLine(csvReader); });
                }
            }
        }

        private FileReadResult TryReadXlsx(string filePath, string worksheetName)
        {
            if (String.IsNullOrWhiteSpace(worksheetName))
            {
                throw new ArgumentOutOfRangeException("worksheetName");
            }

            try
            {
                using (ExcelPackage xlsxFile = new ExcelPackage(new FileInfo(filePath)))
                {
                    ExcelWorksheet worksheet = xlsxFile.Workbook.Worksheets.First(sheet => sheet.Name == worksheetName);
                    int row = 0;
                    return this.TryRead(() => { return this.ReadXlsxRow(worksheet, ++row); });
                }
            }
            catch (IOException ioException)
            {
                FileReadResult result = new FileReadResult()
                {
                    Failed = true
                };
                result.Warnings.Add(ioException.ToString());
                return result;
            }
        }

        protected FileReadResult VerifyHeader(List<string> columnsFromFile, ReadOnlyCollection<ColumnDefinition> knownColumns)
        {
            FileReadResult verifyResult = new FileReadResult();
            if (columnsFromFile == null)
            {
                verifyResult.Failed = true;
                return verifyResult;
            }

            List<string> knownColumnNames = knownColumns.Select(column => column.Name).ToList();
            List<string> missingColumns = knownColumnNames.Except(columnsFromFile).ToList();
            foreach (string dataLabel in missingColumns)
            {
                ColumnDefinition knownColumn = knownColumns.First(column => column.Name == dataLabel);
                if (knownColumn.IsRequired)
                {
                    verifyResult.Failed = true;
                }
                verifyResult.Warnings.Add("- The column '" + dataLabel + "' is not present in the input file." + Environment.NewLine);
            }

            List<string> extraColumnsInFile = columnsFromFile.Except(knownColumnNames).ToList();
            foreach (string dataLabel in extraColumnsInFile)
            {
                verifyResult.Verbose.Add("- The column '" + dataLabel + "' in the input file will be ignored." + Environment.NewLine);
            }

            return verifyResult;
        }

        public abstract void WriteCsv(string filePath);
        public abstract void WriteXlsx(string filePath, string worksheetName);
    }
}
