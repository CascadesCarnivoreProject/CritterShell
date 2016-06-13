using System;
using System.Globalization;

namespace CritterShell.Critters
{
    internal class CritterImage
    {
        public Activity Activity { get; set; }
        public Age Age { get; set; }
        public string Comments { get; set; }
        public Confidence Confidence { get; set; }
        public string File { get; set; }
        public string Folder { get; set; }
        public GroupType GroupType { get; set; }
        public string Identification { get; set; }
        public ImageQuality ImageQuality { get; set; }
        public bool MarkForDeletion { get; set; }
        public string RelativePath { get; set; }
        public string Pelage { get; set; }
        public string Station { get; set; }
        public string Survey { get; set; }
        public DateTime Time { get; set; }
        public TriggerSource TriggerSource { get; set; }

        public string GetSite()
        {
            if (String.IsNullOrWhiteSpace(this.Station))
            {
                return null;
            }
            return this.Station.Substring(0, Math.Min(4, this.Station.Length));
        }

        public void SetTime(string date, string time)
        {
            this.Time = DateTime.ParseExact(date + " " + time, "dd-MMM-yy H:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
        }
    }
}
