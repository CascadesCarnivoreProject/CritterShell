using System;
using System.Diagnostics;

namespace CritterShell.Critters
{
    public class CritterDetection : CritterWithMergeableProperties
    {
        public Activity Activity { get; set; }
        public Age Age { get; set; }
        public string Comments { get; set; }
        public Confidence Confidence { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime EndDateTime { get; set; }
        public string File { get; set; }
        public GroupType GroupType { get; set; }
        public string Identification { get; set; }
        public string Pelage { get; set; }
        public string RelativePath { get; set; }
        public DateTime StartDateTime { get; set; }
        public string Station { get; set; }
        public string Survey { get; set; }
        public TriggerSource TriggerSource { get; set; }
        public TimeSpan UtcOffset { get; set; }

        public CritterDetection()
        {
        }

        public CritterDetection(CritterImage image)
        {
            this.Activity = image.Activity;
            this.Age = image.Age;
            this.Comments = image.Comments;
            this.Confidence = image.Confidence;
            this.Duration = TimeSpan.Zero;
            this.EndDateTime = image.DateTime;
            this.File = image.File;
            this.GroupType = image.GroupType;
            this.Identification = image.Identification;
            this.Pelage = image.Pelage;
            this.RelativePath = image.RelativePath;
            this.StartDateTime = image.DateTime;
            this.Station = image.Station;
            this.Survey = image.Survey;
            this.TriggerSource = image.TriggerSource;
            this.UtcOffset = image.UtcOffset;
        }

        public DateTimeOffset GetStartDateTimeOffset()
        {
            return new DateTimeOffset(this.StartDateTime.AsUnspecifed() + this.UtcOffset, this.UtcOffset);
        }

        public void SetStartAndEndDateTimes(DateTimeOffset newStartTime)
        {
            TimeSpan adjustment = newStartTime - this.GetStartDateTimeOffset();
            this.EndDateTime += adjustment;
            this.StartDateTime = newStartTime.UtcDateTime;
            this.UtcOffset = newStartTime.Offset;
        }

        public bool TryMerge(CritterDetection other, TimeSpan window)
        {
            TimeSpan timeDifference = other.StartDateTime - this.StartDateTime;
            Debug.Assert(timeDifference >= TimeSpan.Zero, "Detections are expected to be ordered by start time.");

            if ((timeDifference > window) ||
                (this.Identification != other.Identification) ||
                (this.Station != other.Station))
            {
                return false;
            }

            // File, Folder, and RelativePath are left pointing to the first image in the detection
            // Survey and TriggerSource are not merged
            // UtcOffset is left as the first image in the detection; doesn't matter if later images have a different offset due to daylight savings
            this.Activity = this.MergeActivity(this.Activity, other.Activity);
            this.Age = this.MergeAge(this.Age, other.Age);
            this.Comments = this.MergeString(this.Comments, other.Comments);
            this.Confidence = this.MergeConfidence(this.Confidence, other.Confidence);
            this.GroupType = this.MergeGroupType(this.GroupType, other.GroupType);
            this.Pelage = this.MergeString(this.Pelage, other.Pelage);

            this.EndDateTime = other.EndDateTime;
            this.Duration = this.EndDateTime - this.StartDateTime;
            return true;
        }
    }
}
