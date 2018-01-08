using System;
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
            return String.Format("{0} images and {1} videos copied, {2} files processed total", this.ImagesCopied, this.VideosCopied, this.FilesProcessed);
        }

        public string ToString(Stopwatch stopwatch)
        {
            return String.Format("{0} images and {1} videos copied, {2} files processed total ({3:0.00} MB/s).", this.ImagesCopied, this.VideosCopied, this.FilesProcessed, this.GetMegabytesPerSecond(stopwatch));
        }
    }
}
