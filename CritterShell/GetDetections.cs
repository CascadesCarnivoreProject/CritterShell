using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;

namespace CritterShell
{
    [Cmdlet(VerbsCommon.Get, "Detections")]
    public class GetDetections : CritterSpreadsheetCmdlet
    {
        [Parameter]
        public SwitchParameter BySite { get; set; }

        [Parameter(Mandatory = true)]
        public string ImageFile { get; set; }

        [Parameter]
        public string ImageWorksheet { get; set; }

        [Parameter]
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
            if (String.IsNullOrEmpty(this.OutputFile))
            {
                this.OutputFile = Path.Combine(Path.GetDirectoryName(this.ImageFile), Path.GetFileNameWithoutExtension(this.ImageFile) + "-detections" + Path.GetExtension(this.ImageFile));
            }
            else
            {
                this.OutputFile = this.CanonicalizePath(this.OutputFile);
            }

            CritterImages critterImages = new CritterImages();
            List<string> importErrors;
            if (critterImages.TryRead(this.ImageFile, this.ImageWorksheet, out importErrors) == false ||
                importErrors.Count > 0)
            {
                foreach (string importError in importErrors)
                {
                    this.WriteWarning(importError);
                }
                this.WriteError(new ErrorRecord(null, "CsvImport", ErrorCategory.InvalidData, this.ImageFile));
                return;
            }

            CritterDetections critterDetections = new CritterDetections(critterImages, this.Window, this.BySite);
            this.WriteOutput(critterDetections);

            this.WriteObject(critterDetections);
        }
    }
}
