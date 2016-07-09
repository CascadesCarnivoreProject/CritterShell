using CritterShell.Gpx;
using System;
using System.IO;
using System.Management.Automation;

namespace CritterShell
{
    [Cmdlet(VerbsData.Convert, "GpxToCsv")]
    public class ConvertGpxToCsv : CritterCmdlet
    {
        [Parameter(Mandatory = true)]
        public string GpxFile { get; set; }

        [Parameter]
        public string OutputFile { get; set; }

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
            critterSign.WriteCsv(this.OutputFile);
        }
    }
}
