using CritterShell.Images;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace CritterShell
{
    [Cmdlet(VerbsCommon.Copy, "CameraFiles")]
    public class CopyCameraFiles : CritterCmdlet, IDisposable
    {
        private CancellationTokenSource cancellationTokenSource;
        private bool disposed;
        private ParallelLoopState loopState;

        [Parameter(HelpMessage = "List of analysis folders to create in the parent of the folder indicated by -To.  Defaults to @(\"Critters\").")]
        public List<string> DefaultFolders { get; set; }

        [Parameter(HelpMessage = @"Path of folder to copy files from.  Defaults to D:\DCIM as that's the most likely drive letter and path for an SD card.")]
        public string From { get; set; }

        [Parameter(HelpMessage = "Whether to use streams or [File]::Copy() for transfering files.  Default is false.")]
        public SwitchParameter Streams { get; set; }

        [Parameter(HelpMessage = "Number of threads to use during copying.  Default is 2.")]
        public int Threads { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Path of folder to copy files to.")]
        public string To { get; set; }

        [Parameter(HelpMessage = "Copy only .jpg files.  Can be useful for shortening copy times by skipping .avi files from cameras operating in hybrid mode.")]
        public SwitchParameter ImagesOnly { get; set; }

        public CopyCameraFiles()
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            this.DefaultFolders = new List<string>() { "Critters" };
            this.disposed = false;
            this.From = @"D:\DCIM";
            this.loopState = null;
            this.Streams = false;
            this.Threads = 2;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.cancellationTokenSource.Dispose();
            }

            this.disposed = true;
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
            string searchPattern = "*.*";
            if (this.ImagesOnly)
            {
                searchPattern = "*" + Constant.File.JpgExtension;
            }

            FileCopyResult copyResult = new FileCopyResult();
            Thread loggingThread = Thread.CurrentThread;
            ParallelOptions parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 2
            };
            Stopwatch stopwatch = new Stopwatch();
            ConcurrentQueue<string> verbose = new ConcurrentQueue<string>();

            Task copy = Task.Run(() =>
            {
                stopwatch.Start();
                Parallel.ForEach(Directory.GetDirectories(this.From), parallelOptions, (string inputSubdirectoryPath, ParallelLoopState loopState) =>
                {
                    if (this.loopState == null)
                    {
                        this.loopState = loopState;
                    }

                    DirectoryInfo inputSubdirectory = new DirectoryInfo(inputSubdirectoryPath);

                    FileInfo[] inputFiles = inputSubdirectory.GetFiles(searchPattern);
                    double gigabytesToCopy = inputFiles.Sum(image => image.Length) / (1024.0 * 1024.0 * 1024.0);
                    verbose.Enqueue(String.Format("Processing {1} files from {2} ({0:0.00}GB)...", gigabytesToCopy, inputFiles.Length, inputSubdirectory.Name));

                    FileCopy fileCopy = new FileCopy();
                    string outputSubdirectoryPath = Path.Combine(this.To, inputSubdirectory.Name);
                    FileCopyResult threadResult;
                    if (this.Streams)
                    {
                        threadResult = fileCopy.StreamAsync(inputFiles, outputSubdirectoryPath, this.cancellationTokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    else
                    {
                        threadResult = fileCopy.Sequential(inputFiles, outputSubdirectoryPath, this.cancellationTokenSource.Token);
                    }
                    copyResult.AccumulateThreadSafe(threadResult);
                });
                stopwatch.Stop();
            });
            while (copy.Status == TaskStatus.Running)
            {
                while (verbose.TryDequeue(out string message))
                {
                    this.WriteVerbose(message);
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(50));
            }

            this.WriteVerbose(copyResult.ToString(stopwatch));
        }

        protected override void StopProcessing()
        {
            base.StopProcessing();
            this.cancellationTokenSource.Cancel();
            if (this.loopState != null)
            {
                this.loopState.Stop();
            }
        }
    }
}
