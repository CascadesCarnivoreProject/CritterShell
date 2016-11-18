using CritterShell.Critters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace CritterShell
{
    internal class CritterImages : SpreadsheetReaderWriter
    {
        private static readonly ReadOnlyCollection<ColumnDefinition> Columns;

        public List<CritterImage> Images { get; private set; }

        static CritterImages()
        {
            CritterImages.Columns = new List<ColumnDefinition>()
            {
                new ColumnDefinition(Constant.ImageColumn.File, true),
                new ColumnDefinition(Constant.ImageColumn.RelativePath, true),
                new ColumnDefinition(Constant.ImageColumn.DateTime, true),
                new ColumnDefinition(Constant.ImageColumn.UtcOffset, true),
                new ColumnDefinition(Constant.ImageColumn.ImageQuality),
                new ColumnDefinition(Constant.ImageColumn.DeleteFlag),
                new ColumnDefinition(Constant.ImageColumn.Survey),
                new ColumnDefinition(Constant.ImageColumn.Station, true),
                new ColumnDefinition(Constant.ImageColumn.TriggerSource),
                new ColumnDefinition(Constant.ImageColumn.Confidence),
                new ColumnDefinition(Constant.ImageColumn.Identification, true),
                new ColumnDefinition(Constant.ImageColumn.Age),
                new ColumnDefinition(Constant.ImageColumn.GroupType),
                new ColumnDefinition(Constant.ImageColumn.Activity),
                new ColumnDefinition(Constant.ImageColumn.Pelage),
                new ColumnDefinition(Constant.ImageColumn.Comments)
            }.AsReadOnly();
        }

        public CritterImages()
        {
            this.Images = new List<CritterImage>();
        }

        protected override FileReadResult TryRead(Func<List<string>> readLine)
        {
            // validate header row against expectations
            List<string> dataLabelsFromHeader = readLine.Invoke();
            FileReadResult readResult = this.VerifyHeader(dataLabelsFromHeader, CritterImages.Columns);
            if (readResult.Failed)
            {
                return readResult;
            }

            // read data
            int fileIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.File);
            int relativePathIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.RelativePath);
            int dateTimeIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.DateTime);
            int utcOffsetIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.UtcOffset);
            int stationIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.Station);
            int identificationIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.Identification);

            int activityIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.Activity);
            int ageIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.Age);
            int commentsIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.Comments);
            int confidenceIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.Confidence);
            int deleteFlagIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.DeleteFlag);
            int groupTypeIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.GroupType);
            int imageQualityIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.ImageQuality);
            int pelageIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.Pelage);
            int triggerSourceIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.TriggerSource);
            int surveyIndex = dataLabelsFromHeader.IndexOf(Constant.ImageColumn.Survey);
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
                // required columns
                image.File = row[fileIndex];
                image.RelativePath = row[relativePathIndex];
                image.DateTime = this.ParseUtcDateTime(row[dateTimeIndex]);
                image.UtcOffset = this.ParseUtcOffset(row[utcOffsetIndex]);
                image.Station = row[stationIndex];
                image.Identification = row[identificationIndex];

                // optional columns
                if (imageQualityIndex != -1)
                {
                    image.ImageQuality = this.ParseEnum<ImageQuality>(row[imageQualityIndex]);
                }
                if (deleteFlagIndex != -1)
                {
                    image.DeleteFlag = this.ParseBoolean(row[deleteFlagIndex]);
                }
                if (triggerSourceIndex != -1)
                {
                    image.TriggerSource = this.ParseEnum<TriggerSource>(row[triggerSourceIndex]);
                }
                if (confidenceIndex != -1)
                {
                    image.Confidence = this.ParseEnum<Confidence>(row[confidenceIndex]);
                }
                if (ageIndex != -1)
                {
                    image.Age = this.ParseEnum<Age>(row[ageIndex]);
                }
                if (groupTypeIndex != -1)
                {
                    image.GroupType = this.ParseEnum<GroupType>(row[groupTypeIndex]);
                }
                if (activityIndex != -1)
                {
                    image.Activity = this.ParseEnum<Activity>(row[activityIndex]);
                }
                if (commentsIndex != -1)
                {
                    image.Comments = row[commentsIndex];
                }
                if (pelageIndex != -1)
                {
                    image.Pelage = row[pelageIndex];
                }
                if (surveyIndex != -1)
                {
                    image.Survey = row[surveyIndex];
                }

                this.Images.Add(image);
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
