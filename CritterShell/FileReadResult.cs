using System.Collections.Generic;

namespace CritterShell
{
    public class FileReadResult
    {
        public bool Failed { get; set; }
        public List<string> Verbose { get; private set; }
        public List<string> Warnings { get; private set; }

        public FileReadResult()
        {
            this.Failed = false;
            this.Verbose = new List<string>();
            this.Warnings = new List<string>();
        }
    }
}
