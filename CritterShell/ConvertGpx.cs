using CritterShell.Critters;
using CritterShell.Gpx;
using System;
using System.IO;
using System.Management.Automation;

namespace CritterShell
{
    [Cmdlet(VerbsData.Convert, "Gpx")]
    public class ConvertGpx : CritterSpreadsheetCmdlet
    {
        [Parameter(HelpMessage = "The type of data contained in the .gpx file.")]
        public DataType DataType { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "The input .gpx file to convert to .csv or .xlsx.")]
        public string GpxFile { get; set; }

        public ConvertGpx()
        {
            this.OutputWorksheet = "waypoints";
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
            switch (this.DataType)
            {
                case DataType.Critter:
                    CritterSigns critterSign = new CritterSigns(gpxFile);
                    this.WriteOutput(critterSign);
                    this.WriteObject(critterSign);
                    break;
                default:
                    GpxSpreadsheet spreadsheetForm = new GpxSpreadsheet(gpxFile);
                    this.WriteOutput(spreadsheetForm);
                    this.WriteObject(gpxFile);
                    break;
            }
        }
    }
}
