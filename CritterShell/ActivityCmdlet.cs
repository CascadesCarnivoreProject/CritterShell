using CritterShell.Critters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;

namespace CritterShell
{
    public class ActivityCmdlet<TActivity> : CritterCmdlet where TActivity : CritterActivity, new()
    {
        [Parameter(Mandatory = true)]
        public string DetectionFile { get; set; }

        [Parameter]
        public string OutputFile { get; set; }

        protected override void ProcessRecord()
        {
            this.DetectionFile = this.CanonicalizePath(this.DetectionFile);
            if (String.IsNullOrEmpty(this.OutputFile))
            {
                this.OutputFile = Path.Combine(Path.GetDirectoryName(this.DetectionFile), Path.GetFileNameWithoutExtension(this.DetectionFile) + "-dielActivity" + Path.GetExtension(this.DetectionFile));
            }
            else
            {
                this.OutputFile = this.CanonicalizePath(this.OutputFile);
            }

            CritterDetections critterDetections = new CritterDetections();
            List<string> importErrors;
            if (critterDetections.TryReadCsv(this.DetectionFile, out importErrors) == false ||
                importErrors.Count > 0)
            {
                foreach (string importError in importErrors)
                {
                    this.WriteWarning(importError);
                }
                this.WriteError(new ErrorRecord(null, "CsvImport", ErrorCategory.InvalidData, this.DetectionFile));
                return;
            }

            ActivityObservations<TActivity> activity = new ActivityObservations<TActivity>(critterDetections);
            activity.WriteCsv(this.OutputFile);
        }
    }
}
