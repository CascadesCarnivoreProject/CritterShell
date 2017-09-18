using CritterShell.Critters;
using System.IO;
using System.Management.Automation;

namespace CritterShell
{
    [Cmdlet(VerbsCommon.Get, "MonthlyActivity")]
    public class GetMonthlyActivity : ActivityCmdlet<CritterMonthlyActivity>
    {
        [Parameter(Mandatory = true, HelpMessage = "Path to the .csv or .xlsx file to load station information from.  Station uptime is used to calculate observations per day.")]
        public string StationFile { get; set; }

        [Parameter(HelpMessage = "The name of the worksheet to load station information from if -StationFile is an .xlsx file.  Not used if -StationFile is a .csv.")]
        public string StationWorksheet { get; set; }

        public GetMonthlyActivity()
        {
            this.OutputWorksheet = "monthly activity";
            this.StationWorksheet = Constant.Excel.DefaultStationsWorksheetName;
        }

        protected override void ProcessRecord()
        {
            this.StationFile = this.CanonicalizePath(this.StationFile);
            base.ProcessRecord();
        }

        protected override bool TryLoadStations(CritterDetections critterDetections, out StationData stations)
        {
            stations = new StationData();
            FileReadResult stationReadResult = stations.TryRead(this.StationFile, this.StationWorksheet);
            if (this.LogRead(stationReadResult))
            {
                this.WriteError(new ErrorRecord(new FileLoadException("Load of station file failed or too many warnings were encountered.", this.StationFile), "StationLoading", ErrorCategory.InvalidData, this.StationFile));
                return false;
            }

            return true;
        }
    }
}
