using CritterShell.Critters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CritterShell
{
    internal class CritterImages : SpreadsheetReaderWriter
    {
        private static readonly ReadOnlyCollection<string> Columns;

        public List<CritterImage> Images { get; private set; }

        static CritterImages()
        {
            CritterImages.Columns = new List<string>()
            {
                Constant.ImageColumn.File,
                Constant.ImageColumn.RelativePath,
                Constant.ImageColumn.DateTime,
                Constant.ImageColumn.UtcOffset,
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

        protected override bool TryRead(Func<List<string>> readLine, out List<string> importErrors)
        {
            // validate header row against expectations
            List<string> dataLabelsFromHeader = readLine.Invoke();
            if (this.VerifyHeader(dataLabelsFromHeader, CritterImages.Columns, out importErrors) == false)
            {
                return false;
            }

            // read data
            int fileIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.File);
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
            for (List<string> row = readLine.Invoke(); row != null; row = readLine.Invoke())
            {
                if (row.Count == CritterImages.Columns.Count - 1)
                {
                    // .csv files are ambiguous in the sense a trailing comma may or may not be present at the end of the line
                    // if the final field has a value this case isn't a concern, but if the final field has no value then there's
                    // no way for the parser to know the exact number of fields in the line
                    row.Add(String.Empty);
                }
                else if (row.Count != CritterImages.Columns.Count)
                {
                    Debug.Assert(false, String.Format("Expected {0} fields in line {1} but found {2}.", CritterImages.Columns.Count, String.Join(",", row), row.Count));
                }

                CritterImage image = new CritterImage();
                image.File = row[fileIndex];
                image.RelativePath = row[relativePathIndex];
                image.DateTime = this.ParseUtcDateTime(row[dateTimeIndex]);
                image.UtcOffset = this.ParseUtcOffset(row[utcOffsetIndex]);
                image.ImageQuality = this.ParseEnum<ImageQuality>(row[imageQualityIndex]);
                image.DeleteFlag = this.ParseBoolean(row[deleteFlagIndex]);
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
