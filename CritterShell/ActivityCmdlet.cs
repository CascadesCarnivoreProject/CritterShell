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
        [Parameter(Mandatory = true, HelpMessage = "Path to the .csv or .xlsx file to load detections from.  This is produced by Get-Detections.")]
        public string DetectionFile { get; set; }

        [Parameter(HelpMessage = "The name of the worksheet to load detections from if -DetectionFile is an .xlsx file.  Not used if -DetectionFile is a .csv.")]
        public string DetectionWorksheet { get; set; }

        [Parameter(HelpMessage = "Groups of stations to include in the output.  See Add-Groups for more information.")]
        public Dictionary<string, List<string>> Groups { get; set; }

        [Parameter(HelpMessage = "By default, activity is in terms of counts.  This switch outputs probabilities instead, where the probability is simply count / n.")]
        public SwitchParameter Probabilities { get; set; }

        [Parameter(HelpMessage = "By default the output time zone is the same as that of the input detections.  Specify this parameter if, for example, the detections use daylight savings but the output needs to be in standard time to, for example, accurately indicate diel activity.")]
        public TimeZoneInfo TimeZone { get; set; }

        [Parameter(HelpMessage = "Include a group containing all stations in the output.")]
        public SwitchParameter Total { get; set; }

        public ActivityCmdlet()
        {
            this.DetectionWorksheet = Constant.Excel.DefaultDetectionsWorksheetName;
            // this.OutputWorksheet is defaulted in derived classes

            // attempt to select the appropriate solar time for the current time zone
            // this will be wrong for some time zones, such as Alaska, with UTC offsets which don't match their geography
            // however, correcting this via a lookup table of overrides is difficult as some time zones span multiple solar time zones
            // better defaults can be added as needed, workaround is simply to specify -SolarTimeZone
            TimeZoneInfo solarTimeForLocalTimeZone = TimeZoneInfo.GetSystemTimeZones().Where(timeZone => timeZone.BaseUtcOffset == TimeZoneInfo.Local.BaseUtcOffset && timeZone.SupportsDaylightSavingTime == false).First();
            this.TimeZone = solarTimeForLocalTimeZone;
        }

        protected override void ProcessRecord()
        {
            // setup
            this.DetectionFile = this.CanonicalizePath(this.DetectionFile);
            this.CanonicalizeOrDefaultOutputFile(this.DetectionFile, "-dielActivity");

            // load detections
            CritterDetections critterDetections = new CritterDetections();
            FileReadResult readResult = critterDetections.TryRead(this.DetectionFile, this.DetectionWorksheet);
            foreach (string message in readResult.Verbose)
            {
                this.WriteVerbose(message);
            }
            foreach (string warning in readResult.Warnings)
            {
                this.WriteWarning(warning);
            }
            if (readResult.Failed || readResult.Warnings.Count > Constant.File.MaximumImportWarnings)
            {
                this.WriteError(new ErrorRecord(new FileLoadException("Too many warnings encountered loading detection file.", this.DetectionFile), "DetectionLoading", ErrorCategory.InvalidData, this.DetectionFile));
                return;
            }

            // convert detections to appropriate solar time
            foreach (CritterDetection detection in critterDetections.Detections)
            {
                if (detection.UtcOffset != this.TimeZone.BaseUtcOffset)
                {
                    DateTimeOffset startDateTime = detection.GetStartDateTimeOffset();
                    detection.SetStartAndEndDateTimes(startDateTime.SetOffset(this.TimeZone.BaseUtcOffset));
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
