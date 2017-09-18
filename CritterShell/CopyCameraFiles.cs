using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace CritterShell
{
    [Cmdlet(VerbsCommon.Copy, "CameraFiles")]
    public class CopyCameraFiles : CritterCmdlet
    {
        [Parameter(HelpMessage = "List of analysis folders to create in the parent of the folder indicated by -To.  Defaults to @(\"Critters\").")]
        public List<string> DefaultFolders { get; set; }

        [Parameter(HelpMessage = @"Path of folder to copy files from.  Defaults to D:\DCIM as that's the most likely drive letter and path for an SD card.")]
        public string From { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Path of folder to copy files to.")]
        public string To { get; set; }

        [Parameter(HelpMessage = "Copy only .jpg files.  Can be useful for shortening copy times by skipping .avi files from cameras operating in hybrid mode.")]
        public SwitchParameter ImagesOnly { get; set; }

        public CopyCameraFiles()
        {
            this.DefaultFolders = new List<string>() { "Critters" };
            this.From = @"D:\DCIM";
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

            string searchPattern = "*.*";
            if (this.ImagesOnly)
            {
                searchPattern = "*" + Constant.File.JpgExtension;
            }

            ParallelOptions parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 2
            };
            Thread loggingThread = Thread.CurrentThread;
            ConcurrentQueue<string> verbose = new ConcurrentQueue<string>();
            int filesProcessed = 0;
            int imagesCopied = 0;
            int videosCopied = 0;
            Parallel.ForEach(Directory.GetDirectories(this.From), parallelOptions, (string inputSubdirectoryPath, ParallelLoopState loopState) =>
                {
                    DirectoryInfo inputSubdirectory = new DirectoryInfo(inputSubdirectoryPath);
                    string outputSubdirectoryPath = Path.Combine(this.To, inputSubdirectory.Name);
                    if (Directory.Exists(outputSubdirectoryPath) == false)
                    {
                        Directory.CreateDirectory(outputSubdirectoryPath);
                    }

                    FileInfo[] inputFiles = inputSubdirectory.GetFiles(searchPattern);
                    double gigabytesToCopy = 1E-9 * inputFiles.Sum(image => image.Length);
                    verbose.Enqueue(String.Format("Processing {0:0.00}GB in {1} files from {2}...", gigabytesToCopy, inputFiles.Length, inputSubdirectory.Name));
                    if (Thread.CurrentThread.ManagedThreadId == loggingThread.ManagedThreadId)
                    {
                        while (verbose.TryDequeue(out string message))
                        {
                            this.WriteVerbose(message);
                        }
                    }

                    foreach (FileInfo inputFile in inputFiles)
                    {
                        string outputFilePath = Path.Combine(outputSubdirectoryPath, inputFile.Name);
                        if (File.Exists(outputFilePath) == false)
                        {
                            File.Copy(inputFile.FullName, outputFilePath);

                            if (String.Equals(inputFile.Extension, Constant.File.JpgExtension, StringComparison.OrdinalIgnoreCase))
                            {
                                ++imagesCopied;
                            }
                            else
                            {
                                ++videosCopied;
                            }

                            if (this.Stopping)
                            {
                                return;
                            }
                        }

                        if (this.Stopping)
                        {
                            loopState.Stop();   
                        }
                    }
                    filesProcessed += inputFiles.Length;
                });

            this.WriteVerbose("{0} images and {1} videos copied, {2} files processed total.", imagesCopied, videosCopied, filesProcessed);
        }
    }
}
