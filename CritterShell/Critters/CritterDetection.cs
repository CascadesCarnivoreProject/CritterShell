using System;
using System.Diagnostics;

namespace CritterShell.Critters
{
    internal class CritterDetection : CritterWithMergeableProperties
    {
        public Activity Activity { get; private set; }
        public Age Age { get; private set; }
        public string Comments { get; private set; }
        public Confidence Confidence { get; private set; }
        public TimeSpan Duration { get; private set; }
        public DateTime EndTime { get; set; }
        public string File { get; private set; }
        public string Folder { get; private set; }
        public GroupType GroupType { get; private set; }
        public string Identification { get; private set; }
        public string Pelage { get; private set; }
        public string RelativePath { get; private set; }
        public DateTime StartTime { get; set; }
        public string Station { get; set; }
        public string Survey { get; private set; }
        public TriggerSource TriggerSource { get; private set; }

        public CritterDetection(CritterImage image)
        {
            this.Activity = image.Activity;
            this.Age = image.Age;
            this.Comments = image.Comments;
            this.Confidence = image.Confidence;
            this.Duration = TimeSpan.Zero;
            this.EndTime = image.Time;
            this.File = image.File;
            this.Folder = image.Folder;
            this.GroupType = image.GroupType;
            this.Identification = image.Identification;
            this.Pelage = image.Pelage;
            this.RelativePath = image.RelativePath;
            this.StartTime = image.Time;
            this.Station = image.Station;
            this.Survey = image.Survey;
            this.TriggerSource = image.TriggerSource;
        }

        public bool TryMerge(CritterDetection other, TimeSpan window)
        {
            TimeSpan timeDifference = other.StartTime - this.StartTime;
            Debug.Assert(timeDifference >= TimeSpan.Zero, "Detections are expected to be ordered by start time.");

            if ((timeDifference > window) ||
                (this.Identification != other.Identification) ||
                (this.Station != other.Station))
            {
                return false;
            }

            // File, Folder, and RelativePath are left pointing to the first image in the detection
            // Survey and TriggerSource are not merged
            this.Activity = this.MergeActivity(this.Activity, other.Activity);
            this.Age = this.MergeAge(this.Age, other.Age);
            this.Comments = this.MergeString(this.Comments, other.Comments);
            this.Confidence = this.MergeConfidence(this.Confidence, other.Confidence);
            this.GroupType = this.MergeGroupType(this.GroupType, other.GroupType);
            this.Pelage = this.MergeString(this.Pelage, other.Pelage);

            this.EndTime = other.EndTime;
            this.Duration = this.EndTime - this.StartTime;
            return true;
        }
    }
}
