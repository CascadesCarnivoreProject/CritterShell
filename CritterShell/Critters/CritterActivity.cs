using System;
using System.Collections.Generic;
using System.Linq;

namespace CritterShell.Critters
{
    public abstract class CritterActivity
    {
        public Dictionary<string, List<int>> DetectionsByIdentification { get; private set; }
        public string Station { get; set; }
        public string Survey { get; set; }

        protected CritterActivity()
        {
            this.DetectionsByIdentification = new Dictionary<string, List<int>>();
        }

        public void Add(CritterDetection detection)
        {
            if (detection.Station != this.Station)
            {
                throw new ArgumentOutOfRangeException("detection", String.Format("Detection for station {0} cannot be added to activity for station {1}.", detection.Station, this.Station));
            }

            // for now, assume detections should be merged across surveys
            if (detection.Survey != this.Survey)
            {
                this.Survey += ", " + detection.Survey;
            }

            this.AddCore(detection);
        }

        protected abstract void AddCore(CritterDetection detection);

        public List<double> GetProbability(string identification)
        {
            List<int> detectionCounts = this.DetectionsByIdentification[identification];
            List<double> probability = new List<double>(this.DetectionsByIdentification.Count);
            double totalDetections = detectionCounts.Sum();
            foreach (int detectionCount in detectionCounts)
            {
                probability.Add((double)detectionCount / totalDetections);
            }
            return probability;
        }
    }
}
