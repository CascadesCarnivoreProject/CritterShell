using System;
using System.Collections.Generic;

namespace CritterShell.Critters
{
    public class CritterDielActivity : CritterActivity
    {
        protected override void AddCore(CritterDetection detection)
        {
            if (this.DetectionsByIdentification.TryGetValue(detection.Identification, out List<int> detections) == false)
            {
                detections = new List<int>(Constant.Time.HoursInDay);
                for (int hour = 0; hour < Constant.Time.HoursInDay; ++hour)
                {
                    detections.Add(0);
                }
                this.DetectionsByIdentification.Add(detection.Identification, detections);
            }

            DateTimeOffset detectionTime = detection.GetStartDateTimeOffset();
            detections[detectionTime.TimeOfDay.Hours]++;
        }
    }
}
