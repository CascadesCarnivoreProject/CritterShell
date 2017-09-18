using System;
using System.Collections.Generic;
using System.Linq;

namespace CritterShell.Critters
{
    public class CritterMonthlyActivity : CritterActivity
    {
        protected override void AddCore(CritterDetection detection)
        {
            if (this.DetectionsByIdentification.TryGetValue(detection.Identification, out List<int> detections) == false)
            {
                detections = new List<int>(Constant.Time.MonthsInYear);
                for (int month = 0; month < Constant.Time.MonthsInYear; ++month)
                {
                    detections.Add(0);
                }
                this.DetectionsByIdentification.Add(detection.Identification, detections);
            }

            DateTimeOffset detectionTime = detection.GetStartDateTimeOffset();
            detections[detectionTime.Date.Month - 1]++;
        }

        public override List<double> GetActivity(string identification, out double totalDetections)
        {
            List<int> monthlyDetections = this.DetectionsByIdentification[identification];
            totalDetections = monthlyDetections.Sum();

            List<double> activity = monthlyDetections.Select(value => (double)value).ToList();
            for (int index = 0; index < monthlyDetections.Count; ++index)
            {
                activity.Add(activity[index] / this.Station.GetUptime(index + 1));
            }
            return activity;
        }

        public override List<double> GetProbability(string identification, out double totalDetections)
        {
            List<double> activity = this.GetActivity(identification, out totalDetections);
            for (int index = 0; index < activity.Count; ++index)
            {
                activity[index] /= totalDetections;
            }
            return activity;
        }
    }
}
