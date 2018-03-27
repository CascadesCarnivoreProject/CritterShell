using System.Diagnostics;
using System.IO;

namespace CritterShell.Images
{
    internal class FileCopyResult
    {
        public long BytesCopied { get; set; }
        public int FilesProcessed { get; set; }
        public int ImagesCopied { get; set; }
        public int VideosCopied { get; set; }

        public FileCopyResult()
        {
            this.BytesCopied = 0;
            this.FilesProcessed = 0;
            this.ImagesCopied = 0;
            this.VideosCopied = 0;
        }

        public void Accumulate(FileInfo fileInfo)
        {
            this.BytesCopied += fileInfo.Length;
            ++this.FilesProcessed;
            if (fileInfo.IsImage())
            {
                ++this.ImagesCopied;
            }
            else if (fileInfo.IsVideo())
            {
                ++this.VideosCopied;
            }
        }

        public void AccumulateThreadSafe(FileCopyResult other)
        {
            lock (this)
            {
                this.BytesCopied += other.BytesCopied;
                this.FilesProcessed += other.FilesProcessed;
                this.ImagesCopied += other.ImagesCopied;
                this.VideosCopied += other.VideosCopied;
            }
        }

        public double GetMegabytesPerSecond(Stopwatch stopwatch)
        {
            return (double)this.BytesCopied / (1024.0 * 1024.0 * stopwatch.Elapsed.TotalSeconds);
        }

        public override string ToString()
        {
            string imageResult = this.ImagesCopied + " images and ";
            if (this.ImagesCopied == 1)
            {
                imageResult = "1 image and ";
            }

            string videoResult = this.VideosCopied + " videos copied, ";
            if (this.VideosCopied == 1)
            {
                videoResult = "1 video copied, ";
            }

            string filesResult = this.FilesProcessed + " files processed total, ";
            if (this.FilesProcessed == 1)
            {
                filesResult = " 1 file processed total, ";
            }

            string bytesCopied;
            double gigabytesCopied = this.BytesCopied / (1024.0 * 1024.0 * 1024.0);
            if (gigabytesCopied >= 0.005)
            {
                bytesCopied = gigabytesCopied.ToString("0.00") + " GB copied";
            }
            else
            {
                double megabytesCopied = this.BytesCopied / (1024.0 * 1024.0);
                bytesCopied = megabytesCopied.ToString("0.00") + " MB copied";
            }

            return imageResult + videoResult + filesResult + bytesCopied;
        }

        public string ToString(Stopwatch stopwatch)
        {
            return this.ToString() + " ("  + this.GetMegabytesPerSecond(stopwatch).ToString("0.00") + " MB/s)";
        }
    }
}
