using CritterShell.Critters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace CritterShell
{
    public class ActivityCmdlet<TActivity> : CritterSpreadsheetCmdlet where TActivity : CritterActivity, new()
    {
        [Parameter(Mandatory = true)]
        public string DetectionFile { get; set; }

        [Parameter]
        public string DetectionWorksheet { get; set; }

        [Parameter]
        public Dictionary<string, List<string>> Groups { get; set; }

        [Parameter]
        public SwitchParameter Probabilities { get; set; }

        [Parameter]
        public TimeZoneInfo SolarTimeZone { get; set; }

        [Parameter]
        public SwitchParameter Total { get; set; }

        public ActivityCmdlet()
        {
            this.DetectionWorksheet = Constant.Excel.DefaultDetectionsWorksheetName;
            this.OutputWorksheet = "critter activity";

            // attempt to select the appropriate solar time for the current time zone
            // this will be wrong for some time zones, such as Alaska, with UTC offsets which don't match their geography
            // however, correcting this via a lookup table of overrides is difficult as some time zones span multiple solar time zones
            // better defaults can be added as needed, workaround is simply to specify -SolarTimeZone
            TimeZoneInfo solarTimeForLocalTimeZone = TimeZoneInfo.GetSystemTimeZones().Where(timeZone => timeZone.BaseUtcOffset == TimeZoneInfo.Local.BaseUtcOffset && timeZone.SupportsDaylightSavingTime == false).First();
            this.SolarTimeZone = solarTimeForLocalTimeZone;
        }

        protected override void ProcessRecord()
        {
            // setup
            this.DetectionFile = this.CanonicalizePath(this.DetectionFile);
            if (String.IsNullOrEmpty(this.OutputFile))
            {
                this.OutputFile = Path.Combine(Path.GetDirectoryName(this.DetectionFile), Path.GetFileNameWithoutExtension(this.DetectionFile) + "-dielActivity" + Path.GetExtension(this.DetectionFile));
            }
            else
            {
                this.OutputFile = this.CanonicalizePath(this.OutputFile);
            }

            // load detections
            CritterDetections critterDetections = new CritterDetections();
            List<string> importErrors;
            if (critterDetections.TryRead(this.DetectionFile, this.DetectionWorksheet, out importErrors) == false ||
                importErrors.Count > 0)
            {
                foreach (string importError in importErrors)
                {
                    this.WriteWarning(importError);
                }
                this.WriteError(new ErrorRecord(null, "CsvImport", ErrorCategory.InvalidData, this.DetectionFile));
                return;
            }

            // convert detections to appropriate solar time
            foreach (CritterDetection detection in critterDetections.Detections)
            {
                if (detection.UtcOffset != this.SolarTimeZone.BaseUtcOffset)
                {
                    DateTimeOffset startDateTime = detection.GetStartDateTimeOffset();
                    detection.SetStartAndEndDateTimes(startDateTime.SetOffset(this.SolarTimeZone.BaseUtcOffset));
                }
            }
            
            // collect activity
            ActivityObservations<TActivity> activity = new ActivityObservations<TActivity>(critterDetections, this.Groups);
            activity.WriteProbabilities = this.Probabilities;
            activity.WriteTotal = this.Total;
            this.WriteOutput(activity);

            this.WriteObject(activity);
        }
    }
}
