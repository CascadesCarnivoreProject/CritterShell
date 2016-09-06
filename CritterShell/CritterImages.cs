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
        private static readonly ReadOnlyCollection<string> CsvColumns;

        public List<CritterImage> Images { get; private set; }

        static CritterImages()
        {
            CritterImages.CsvColumns = new List<string>()
            {
                Constant.ImageColumn.File,
                Constant.ImageColumn.Folder,
                Constant.ImageColumn.RelativePath,
                Constant.ImageColumn.DateTime,
                Constant.ImageColumn.UtcOffset,
                Constant.ImageColumn.Date,
                Constant.ImageColumn.Time,
                Constant.ImageColumn.ImageQuality,
                Constant.ImageColumn.DeleteFlag,
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
                    // validate .csv file headers against the expectations
                    // Date and Time columns in the .csv are redundant with the DateTime and UtcOffset columns and are ignored
                    List<string> dataLabelsFromHeader = this.ReadAndParseLine(csvReader);
                    List<string> dataLabelsInCritterImageButNotInHeader = CritterImages.CsvColumns.Except(dataLabelsFromHeader).ToList();
                    foreach (string dataLabel in dataLabelsInCritterImageButNotInHeader)
                    {
                        importErrors.Add("- A column with the header '" + dataLabel + "' is required for a critter image but nothing matches that in the .csv file." + Environment.NewLine);
                    }
                    List<string> dataLabelsInHeaderButNotCritterImage = dataLabelsFromHeader.Except(CritterImages.CsvColumns).ToList();
                    dataLabelsInHeaderButNotCritterImage.Remove(Constant.ImageColumn.Date);
                    dataLabelsInHeaderButNotCritterImage.Remove(Constant.ImageColumn.Time);
                    foreach (string dataLabel in dataLabelsInHeaderButNotCritterImage)
                    {
                        importErrors.Add("- A column with the header '" + dataLabel + "' is present in the .csv file but nothing matches that in the critter image schema." + Environment.NewLine);
                    }

                    if (importErrors.Count > 0)
                    {
                        return false;
                    }

                    // read image updates from the CSV file
                    int fileIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.File);
                    int folderIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.Folder);
                    int relativePathIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.RelativePath);
                    int dateTimeIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.DateTime);
                    int utcOffsetIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.UtcOffset);
                    int imageQualityIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.ImageQuality);
                    int deleteFlagIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.DeleteFlag);
                    int surveyIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.Survey);
                    int stationIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.Station);
                    int triggerSourceIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.TriggerSource);
                    int confidenceIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.Confidence);
                    int identificationIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.Identification);
                    int ageIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.Age);
                    int groupTypeIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.GroupType);
                    int activityIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.Activity);
                    int pelageIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.Pelage);
                    int commentsIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.Comments);
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
                        image.File = row[fileIndex];
                        image.Folder = row[folderIndex];
                        image.RelativePath = row[relativePathIndex];
                        image.DateTime = this.ParseUtcDateTime(row[dateTimeIndex]);
                        image.UtcOffset = this.ParseUtcOffset(row[utcOffsetIndex]);
                        image.ImageQuality = this.ParseEnum<ImageQuality>(row[imageQualityIndex]);
                        image.DeleteFlag = String.IsNullOrWhiteSpace(row[deleteFlagIndex]) ? false : Boolean.Parse(row[deleteFlagIndex]);
                        image.Survey = row[surveyIndex];
                        image.Station = row[stationIndex];
                        image.TriggerSource = this.ParseEnum<TriggerSource>(row[triggerSourceIndex]);
                        image.Confidence = this.ParseEnum<Confidence>(row[confidenceIndex]);
                        image.Identification = row[identificationIndex];
                        image.Age = this.ParseEnum<Age>(row[ageIndex]);
                        image.GroupType = this.ParseEnum<GroupType>(row[groupTypeIndex]);
                        image.Activity = this.ParseEnum<Activity>(row[activityIndex]);
                        image.Pelage = row[pelageIndex];
                        image.Comments = row[commentsIndex];
                        this.Images.Add(image);
                    }

                    return true;
                }
            }
        }
    }
}
