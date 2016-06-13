using System;
using System.Collections.Generic;
using System.IO;

namespace CritterShell
{
    /// <summary>
    /// Base functionality for reading and writing .csv files.
    /// </summary>
    internal class CsvReaderWriter
    {
        protected string AddColumnValue(DateTime value)
        {
            return this.AddColumnValue(value.ToString(Constant.DateTimeUtcFormat));
        }

        protected string AddColumnValue(double value)
        {
            return this.AddColumnValue(value.ToString());
        }

        // Check if there is any Quotation Mark '"', a Comma ',', a Line Feed \x0A,  or Carriage Return \x0D
        // and escape it as needed
        protected string AddColumnValue(string value)
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

        protected TEnum ParseEnum<TEnum>(string value) where TEnum : struct, IComparable, IConvertible, IFormattable
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                return default(TEnum);
            }
            return (TEnum)Enum.Parse(typeof(TEnum), value, true);
        }

        protected List<string> ReadAndParseLine(StreamReader csvReader)
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
    }
}
