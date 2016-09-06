using System;
using System.Collections.Generic;

namespace CritterShell.Critters
{
    public class CritterMonthlyActivity : CritterActivity
    {
        protected override void AddCore(CritterDetection detection)
        {
            List<int> detections;
            if (this.DetectionsByIdentification.TryGetValue(detection.Identification, out detections) == false)
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
    }
}
