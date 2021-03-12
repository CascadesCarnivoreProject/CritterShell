using CritterShell.Images;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace CritterShell
{
    [Cmdlet(VerbsCommon.Copy, "CameraFiles")]
    public class CopyCameraFiles : CritterCmdlet
    {
        private readonly FileCopy fileCopy;

        [Parameter(HelpMessage = "List of analysis folders to create in the parent of the folder indicated by -To.  Defaults to @(\"Critters\").")]
        public List<string> DefaultFolders { get; set; }

        [Parameter(HelpMessage = @"Path of folder to copy files from.  Defaults to D:\DCIM as that's the most likely drive letter and path for an SD card.")]
        public string From { get; set; }

        [Parameter(HelpMessage = "Number of threads to use during copying.  Default is the number of logical processors.")]
        public int Threads { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Path of folder to copy files to.")]
        public string To { get; set; }

        [Parameter(HelpMessage = "Copy only .jpg files.  Can be useful for shortening copy times by skipping .avi files from cameras operating in hybrid mode.")]
        public SwitchParameter ImagesOnly { get; set; }

        public CopyCameraFiles()
        {
            this.DefaultFolders = new List<string>() { "Critters" };
            this.fileCopy = new FileCopy();
            this.From = @"D:\DCIM";
            this.Threads = Environment.ProcessorCount;
        }

        protected override void ProcessRecord()
        {
            this.To = this.CanonicalizePath(this.To);
            if (Directory.Exists(this.To) == false)
            {
                Directory.CreateDirectory(this.To);
            }

            if (this.DefaultFolders != null)
            {
                foreach (string defaultFolderName in this.DefaultFolders)
                {
                    string defaultFolderPath = Path.Combine(Path.GetDirectoryName(this.To), defaultFolderName);
                    if (Directory.Exists(defaultFolderPath) == false)
                    {
                        Directory.CreateDirectory(defaultFolderPath);
                    }
                }
            }

            // for performance considerations see, among others,
            //   https://blogs.technet.microsoft.com/markrussinovich/2008/02/04/inside-vista-sp1-file-copy-improvements/
            //   https://stackoverflow.com/questions/3185607/how-to-implement-a-performant-filecopy-method-in-c-sharp-from-a-network-share
            // 
            // PNY Elite Performance (95 MB/s) -> Samsung EVO 850 (4.1 GB/s)
            // method       threads   speed, MB/s
            // File.Copy()  2         16.3
            // streams      2         25-45, typically ~30
            // streams      4         TBD
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string searchPattern = this.ImagesOnly ? "*.*" : "*" + Constant.File.JpgExtension;
            Task<FileCopyResult> copy = this.fileCopy.CopySubdirectoriesAsync(this.From, searchPattern, this.Threads, this.To);

            // pump messages while copy is running
            while (copy.IsCompleted == false)
            {
                while (this.fileCopy.Errors.TryDequeue(out Exception error))
                {
                    this.WriteError(new ErrorRecord(error, "Copy", ErrorCategory.ReadError, copy));
                }
                while (this.fileCopy.VerboseMessages.TryDequeue(out string message))
                {
                    this.WriteVerbose(message);
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(50));
            }

            // pump any messages which might have been queued while the last iteration of the previous loop was waiting
            while (this.fileCopy.Errors.TryDequeue(out Exception error))
            {
                this.WriteError(new ErrorRecord(error, "Copy", ErrorCategory.ReadError, copy));
            }
            while (this.fileCopy.VerboseMessages.TryDequeue(out string message))
            {
                this.WriteVerbose(message);
            }

            // indicate copy result
            // If copying encountered an exception GetResult() rethrows the exception.
            stopwatch.Stop();
            this.WriteVerbose(copy.GetAwaiter().GetResult().ToString(stopwatch));
        }

        protected override void StopProcessing()
        {
            base.StopProcessing();
            this.fileCopy.ShouldExitCurrentIteration = true;
        }
    }
}
