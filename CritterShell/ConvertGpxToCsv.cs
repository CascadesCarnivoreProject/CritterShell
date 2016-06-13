using CritterShell.Gpx;
using System.IO;
using System.Management.Automation;

namespace CritterShell
{
    [Cmdlet(VerbsData.Convert, "GpxToCsv")]
    public class ConvertGpxToCsv : CritterCmdlet
    {
        [Parameter(Mandatory = true)]
        public string GpxFile { get; set; }

        protected override void ProcessRecord()
        {
            this.GpxFile = this.CanonicalizePath(this.GpxFile);
            GpxFile gpxFile = new GpxFile(this.GpxFile);
            CritterSigns critterSign = new CritterSigns(gpxFile);

            string csvFilePath = Path.Combine(Path.GetDirectoryName(this.GpxFile), Path.GetFileNameWithoutExtension(this.GpxFile) + Constant.Csv.Extension);
            critterSign.WriteCsv(csvFilePath);
        }
    }
}
