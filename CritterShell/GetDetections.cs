using System;
using System.IO;
using System.Management.Automation;

namespace CritterShell
{
    [Cmdlet(VerbsCommon.Get, "Detections")]
    public class GetDetections : CritterSpreadsheetCmdlet
    {
        [Parameter(HelpMessage = "Truncate station names to only their first four characters, as that's normally the indicator of the site where a station's located.")]
        public SwitchParameter BySite { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Path to .csv or .xlsx file listing analyzed images.")]
        public string ImageFile { get; set; }

        [Parameter(HelpMessage = "The name of the worksheet to read images from if -ImageFile is an .xlsx file.  Not used if -ImageFile is a .csv.  Defaults to 'images'.")]
        public string ImageWorksheet { get; set; }

        [Parameter(HelpMessage = "The interval within which images with the same analysis are merged form a detection.  Defaults to five minutes.")]
        public TimeSpan Window { get; set; }

        public GetDetections()
        {
            this.ImageWorksheet = "images";
            this.OutputWorksheet = Constant.Excel.DefaultDetectionsWorksheetName;
            this.Window = Constant.DefaultDetectionMergeWindow;
        }

        protected override void ProcessRecord()
        {
            this.ImageFile = this.CanonicalizePath(this.ImageFile);
            this.CanonicalizeOrDefaultOutputFile(this.ImageFile, "-detections");

            CritterImages critterImages = new CritterImages();
            FileReadResult readResult = critterImages.TryRead(this.ImageFile, this.ImageWorksheet);
            foreach (string importMessage in readResult.Verbose)
            {
                this.WriteVerbose(importMessage);
            }
            foreach (string importError in readResult.Warnings)
            {
                this.WriteWarning(importError);
            }
            if (readResult.Failed || readResult.Warnings.Count > Constant.File.MaximumImportWarnings)
            {
                this.WriteError(new ErrorRecord(new FileLoadException("Too many warnings encountered loading image file.", this.ImageFile), "ImageLoading", ErrorCategory.InvalidData, this.ImageFile));
                return;
            }

            CritterDetections critterDetections = new CritterDetections(critterImages, this.Window, this.BySite);
            this.WriteOutput(critterDetections);

            this.WriteObject(critterDetections);
        }
    }
}
