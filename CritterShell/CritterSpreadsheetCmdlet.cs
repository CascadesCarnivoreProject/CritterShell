using System;
using System.IO;
using System.Management.Automation;

namespace CritterShell
{
    public class CritterSpreadsheetCmdlet : CritterCmdlet
    {
        [Parameter]
        public string OutputFile { get; set; }

        [Parameter]
        public string OutputWorksheet { get; set; }

        protected void WriteOutput(SpreadsheetReaderWriter outputWriter)
        {
            if (String.Equals(Path.GetExtension(this.OutputFile), Constant.Csv.Extension, StringComparison.OrdinalIgnoreCase))
            {
                outputWriter.WriteCsv(this.OutputFile);
            }
            else if (String.Equals(Path.GetExtension(this.OutputFile), Constant.Excel.Extension, StringComparison.OrdinalIgnoreCase))
            {
                outputWriter.WriteXlsx(this.OutputFile, this.OutputWorksheet);
            }
            else
            {
                this.WriteError(new ErrorRecord(null, "Output", ErrorCategory.InvalidData, String.Format("Unknown output file type {0}.", Path.GetExtension(this.OutputFile))));
            }
        }
    }
}
