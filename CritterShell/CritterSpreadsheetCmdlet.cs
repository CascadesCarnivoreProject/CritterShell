using System;
using System.IO;
using System.Management.Automation;

namespace CritterShell
{
    public class CritterSpreadsheetCmdlet : CritterCmdlet
    {
        [Parameter(HelpMessage = "Path to the .csv or .xlsx file to write output to.  If not specified this is derived from the input file.")]
        public string OutputFile { get; set; }

        [Parameter(HelpMessage = "The name of the worksheet to output to from if -OutputFile is an .xlsx file.  Not used if -OutputFile is a .csv.")]
        public string OutputWorksheet { get; set; }

        protected void CanonicalizeOrDefaultOutputFile(string inputFile, string csvFileNameSuffix)
        {
            if (String.IsNullOrEmpty(this.OutputFile))
            {
                string inputFileExtension = Path.GetExtension(inputFile);
                string inputFileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputFile);
                if (String.Equals(inputFileExtension, Constant.Csv.Extension))
                {
                    inputFileNameWithoutExtension += csvFileNameSuffix;
                }
                this.OutputFile = Path.Combine(Path.GetDirectoryName(inputFile), inputFileNameWithoutExtension + inputFileExtension);
            }
            else
            {
                this.OutputFile = this.CanonicalizePath(this.OutputFile);
            }
        }

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
