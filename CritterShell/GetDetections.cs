using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;

namespace CritterShell
{
    [Cmdlet(VerbsCommon.Get, "Detections")]
    public class GetDetections : CritterCmdlet
    {
        [Parameter]
        public SwitchParameter BySite { get; set; }

        [Parameter(Mandatory = true)]
        public string ImageFile { get; set; }

        [Parameter]
        public TimeSpan Window { get; set; }

        public GetDetections()
        {
            this.Window = Constant.DefaultDetectionMergeWindow;
        }

        protected override void ProcessRecord()
        {
            this.ImageFile = this.CanonicalizePath(this.ImageFile);

            CritterImages critterImages = new CritterImages();
            List<string> importErrors;
            if (critterImages.TryReadCsv(this.ImageFile, out importErrors) == false ||
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
            string detectionFilePath = Path.Combine(Path.GetDirectoryName(this.ImageFile), Path.GetFileNameWithoutExtension(this.ImageFile) + "-detections" + Path.GetExtension(this.ImageFile));
            critterDetections.WriteCsv(detectionFilePath);
        }
    }
}
