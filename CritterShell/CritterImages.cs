using CritterShell.Critters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CritterShell
{
    internal class CritterImages : CsvReaderWriter
    {
        public static readonly ReadOnlyCollection<string> CsvColumns;

        public List<CritterImage> Images { get; private set; }

        static CritterImages()
        {
            CritterImages.CsvColumns = new List<string>()
            {
                Constant.ImageColumn.File,
                Constant.ImageColumn.Folder,
                Constant.ImageColumn.RelativePath,
                Constant.ImageColumn.Date,
                Constant.ImageColumn.Time,
                Constant.ImageColumn.ImageQuality,
                Constant.ImageColumn.MarkForDeletion,
                Constant.ImageColumn.Survey,
                Constant.ImageColumn.Station,
                Constant.ImageColumn.TriggerSource,
                Constant.ImageColumn.Confidence,
                Constant.ImageColumn.Identification,
                Constant.ImageColumn.Age,
                Constant.ImageColumn.GroupType,
                Constant.ImageColumn.Activity,
                Constant.ImageColumn.Pelage,
                Constant.ImageColumn.Comments
            }.AsReadOnly();
        }

        public CritterImages()
        {
            this.Images = new List<CritterImage>();
        }

        public bool TryReadCsv(string filePath, out List<string> importErrors)
        {
            importErrors = new List<string>();

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader csvReader = new StreamReader(stream))
                {
                    // validate CSV file headers against the expectations
                    List<string> dataLabelsFromHeader = this.ReadAndParseLine(csvReader);
                    List<string> dataLabelsInImageDatabaseButNotInHeader = CritterImages.CsvColumns.Except(dataLabelsFromHeader).ToList();
                    foreach (string dataLabel in dataLabelsInImageDatabaseButNotInHeader)
                    {
                        importErrors.Add("- A column with the DataLabel '" + dataLabel + "' is present in the database but nothing matches that in the CSV file." + Environment.NewLine);
                    }
                    List<string> dataLabelsInHeaderButNotImageDatabase = dataLabelsFromHeader.Except(CritterImages.CsvColumns).ToList();
                    foreach (string dataLabel in dataLabelsInHeaderButNotImageDatabase)
                    {
                        importErrors.Add("- A column with the DataLabel '" + dataLabel + "' is present in the CSV file but nothing matches that in the database." + Environment.NewLine);
                    }

                    if (importErrors.Count > 0)
                    {
                        return false;
                    }

                    // read image updates from the CSV file
                    for (List<string> row = this.ReadAndParseLine(csvReader); row != null; row = this.ReadAndParseLine(csvReader))
                    {
                        if (row.Count == CritterImages.CsvColumns.Count - 1)
                        {
                            // .csv files are ambiguous in the sense a trailing comma may or may not be present at the end of the line
                            // if the final field has a value this case isn't a concern, but if the final field has no value then there's
                            // no way for the parser to know the exact number of fields in the line
                            row.Add(String.Empty);
                        }
                        else if (row.Count != CritterImages.CsvColumns.Count)
                        {
                            Debug.Assert(false, String.Format("Expected {0} fields in line {1} but found {2}.", CritterImages.CsvColumns.Count, String.Join(",", row), row.Count));
                        }

                        CritterImage image = new CritterImage();
                        image.File = row[0];
                        image.Folder = row[1];
                        image.RelativePath = row[2];
                        image.SetTime(row[3], row[4]);
                        image.ImageQuality = this.ParseEnum<ImageQuality>(row[5]);
                        image.MarkForDeletion = String.IsNullOrWhiteSpace(row[6]) ? false : Boolean.Parse(row[6]);
                        image.Survey = row[7];
                        image.Station = row[8];
                        image.TriggerSource = this.ParseEnum<TriggerSource>(row[9]);
                        image.Confidence = this.ParseEnum<Confidence>(row[10]);
                        image.Identification = row[11];
                        image.Age = this.ParseEnum<Age>(row[12]);
                        image.GroupType = this.ParseEnum<GroupType>(row[13]);
                        image.Activity = this.ParseEnum<Activity>(row[14]);
                        image.Pelage = row[15];
                        image.Comments = row[16];
                        this.Images.Add(image);
                    }

                    return true;
                }
            }
        }
    }
}
