using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace CritterShell
{
    [Cmdlet(VerbsCommon.Copy, "CameraFiles")]
    public class CopyCameraFiles : CritterCmdlet
    {
        [Parameter]
        public List<string> DefaultFolders { get; set; }

        [Parameter]
        public string From { get; set; }

        [Parameter(Mandatory = true)]
        public string To { get; set; }

        [Parameter]
        public SwitchParameter ImagesOnly { get; set; }

        public CopyCameraFiles()
        {
            this.DefaultFolders = new List<string>() { "Critters", "Redundants" };
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

            int filesProcessed = 0;
            int imagesCopied = 0;
            int videosCopied = 0;
            foreach (string inputSubdirectoryPath in Directory.GetDirectories(this.From))
            {
                DirectoryInfo inputSubdirectory = new DirectoryInfo(inputSubdirectoryPath);
                string outputSubdirectoryPath = Path.Combine(this.To, inputSubdirectory.Name);
                if (Directory.Exists(outputSubdirectoryPath) == false)
                {
                    Directory.CreateDirectory(outputSubdirectoryPath);
                }

                FileInfo[] inputFiles = inputSubdirectory.GetFiles(searchPattern);
                double gigabytesToCopy = 1E-9 * inputFiles.Sum(image => image.Length);
                this.WriteVerbose("Processing {0:0.00}GB in {1} files from {2}...", gigabytesToCopy, inputFiles.Length, inputSubdirectory.Name);
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
                }
                filesProcessed += inputFiles.Length;
            }

            this.WriteVerbose("{0} images and {1} videos copied, {2} files processed total.", imagesCopied, videosCopied, filesProcessed);
        }
    }
}
