using CritterShell.Gpx;
using System;
using System.IO;
using System.Management.Automation;

namespace CritterShell
{
    [Cmdlet(VerbsData.Convert, "Gpx")]
    public class ConvertGpx : CritterSpreadsheetCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "The input .gpx file to convert to .csv or .xlsx.")]
        public string GpxFile { get; set; }

        public ConvertGpx()
        {
            this.OutputWorksheet = "critter sign";
        }

        protected override void ProcessRecord()
        {
            this.GpxFile = this.CanonicalizePath(this.GpxFile);
            if (String.IsNullOrEmpty(this.OutputFile))
            {
                this.OutputFile = Path.Combine(Path.GetDirectoryName(this.GpxFile), Path.GetFileNameWithoutExtension(this.GpxFile) + Constant.Csv.Extension);
            }
            else
            {
                this.OutputFile = this.CanonicalizePath(this.OutputFile);
            }

            GpxFile gpxFile = new GpxFile(this.GpxFile);
            CritterSigns critterSign = new CritterSigns(gpxFile);
            this.WriteOutput(critterSign);

            this.WriteObject(critterSign);
        }
    }
}
