using System;

namespace CritterShell.Critters
{
    public class CritterImage
    {
        public Activity Activity { get; set; }
        public Age Age { get; set; }
        public string Comments { get; set; }
        public Confidence Confidence { get; set; }
        public DateTime DateTime { get; set; }
        public string File { get; set; }
        public string Folder { get; set; }
        public GroupType GroupType { get; set; }
        public string Identification { get; set; }
        public ImageQuality ImageQuality { get; set; }
        public bool DeleteFlag { get; set; }
        public string RelativePath { get; set; }
        public string Pelage { get; set; }
        public string Station { get; set; }
        public string Survey { get; set; }
        public TriggerSource TriggerSource { get; set; }
        public TimeSpan UtcOffset { get; set; }
    }
}
