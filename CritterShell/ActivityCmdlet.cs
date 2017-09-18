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

        public ActivityCmdlet()
        {
            this.DetectionWorksheet = Constant.Excel.DefaultDetectionsWorksheetName;
            // this.OutputWorksheet is defaulted in derived classes

            // attempt to select the appropriate solar time for the current time zone
            // This will be wrong for some time zones, such as Alaska, with UTC offsets which don't match their geography.
            // However, correcting this via a lookup table of overrides is difficult as some time zones span multiple solar time zones.
            // Better defaults can be added as needed, workaround is simply to specify -SolarTimeZone.
            TimeZoneInfo solarTimeForLocalTimeZone = TimeZoneInfo.GetSystemTimeZones().Where(timeZone => timeZone.BaseUtcOffset == TimeZoneInfo.Local.BaseUtcOffset && timeZone.SupportsDaylightSavingTime == false).First();
            this.TimeZone = solarTimeForLocalTimeZone;
        }

        protected virtual bool TryLoadStations(CritterDetections critterDetections, out StationData stations)
        {
            stations = new StationData();
            foreach (string stationID in critterDetections.Detections.Select(detection => detection.Station).Distinct())
            {
                stations.Stations.Add(new Station(stationID, stationID));
            }
            return true;
        }

        protected bool LogRead(FileReadResult detectionReadResult)
        {
            foreach (string message in detectionReadResult.Verbose)
            {
                this.WriteVerbose(message);
            }
            foreach (string warning in detectionReadResult.Warnings)
            {
                this.WriteWarning(warning);
            }
            return detectionReadResult.Failed || detectionReadResult.Warnings.Count > Constant.File.MaximumImportWarnings;
        }

        protected override void ProcessRecord()
        {
            // setup
            this.DetectionFile = this.CanonicalizePath(this.DetectionFile);
            this.CanonicalizeOrDefaultOutputFile(this.DetectionFile, "-dielActivity");

            // load detections
            CritterDetections critterDetections = new CritterDetections();
            FileReadResult detectionReadResult = critterDetections.TryRead(this.DetectionFile, this.DetectionWorksheet);
            if (this.LogRead(detectionReadResult))
            {
                this.WriteError(new ErrorRecord(new FileLoadException("Load of detection file failed or too many warnings were encountered.", this.DetectionFile), "DetectionLoading", ErrorCategory.InvalidData, this.DetectionFile));
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

            // load stations
            // Default implementation of TryLoadStations() always returns true, so no error logging is done here.  Overrides of
            // TryLoadStations() therefore should perform their own logging.
            if (this.TryLoadStations(critterDetections, out StationData stations) == false)
            {
                return;
            }

            // collect activity
            ActivityObservations<TActivity> activity = new ActivityObservations<TActivity>(critterDetections, stations, this.Groups)
            {
                WriteProbabilities = this.Probabilities
            };
            this.WriteOutput(activity);

            this.WriteObject(activity);
        }
    }
}
