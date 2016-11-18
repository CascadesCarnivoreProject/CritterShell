﻿using System.Collections.Generic;
using System.Linq;

namespace CritterShell.Critters
{
    public abstract class CritterActivity
    {
        public Dictionary<string, List<int>> DetectionsByIdentification { get; private set; }
        public string Station { get; set; }
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

        public List<double> GetProbability(string identification, out int totalDetections)
        {
            List<int> detectionCounts = this.DetectionsByIdentification[identification];
            List<double> probability = new List<double>(this.DetectionsByIdentification.Count);
            totalDetections = detectionCounts.Sum();
            foreach (int detectionCount in detectionCounts)
            {
                probability.Add((double)detectionCount / (double)totalDetections);
            }
            return probability;
        }
    }
}
