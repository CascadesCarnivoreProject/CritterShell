using System.Collections.Generic;
using System.Linq;

namespace CritterShell.Critters
{
    public abstract class CritterActivity
    {
        public Dictionary<string, List<int>> DetectionsByIdentification { get; private set; }
        public Station Station { get; set; }
        public List<string> Surveys { get; set; }

        protected CritterActivity()
        {
            this.DetectionsByIdentification = new Dictionary<string, List<int>>();
            this.Surveys = new List<string>();
        }

        public void Add(CritterDetection detection)
        {
            if (this.Surveys.Contains(detection.Survey) == false)
            {
                this.Surveys.Add(detection.Survey);
            }

            this.AddCore(detection);
        }

        protected abstract void AddCore(CritterDetection detection);

        public virtual List<double> GetActivity(string identification, out double totalDetections)
        {
            List<int> detections = this.DetectionsByIdentification[identification];
            totalDetections = detections.Sum();
            return detections.Select(value => (double)value).ToList();
        }

        public virtual List<double> GetProbability(string identification, out double totalDetections)
        {
            List<double> probabilities = this.GetActivity(identification, out totalDetections);
            for (int index = 0; index < probabilities.Count; ++index)
            {
                probabilities[index] /= totalDetections;
            }
            return probabilities;
        }
    }
}
