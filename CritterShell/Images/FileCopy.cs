using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CritterShell.Images
{
    internal class FileCopy
    {
        private const long BufferSizeLimit = 256 * 1024 * 1024; // 256MB, must be less than 2GB
        private const int DefaultBufferSize = 10 * 1024 * 1024; // 10MB
        private const int MaximumFilesAllocated = 10;
        private const int ReadBlockSize = 1024 * 1024;
        private const int WriteBlockSize = 1024 * 1024;

        public ConcurrentQueue<Exception> Errors { get; private set; }
        public bool ShouldExitCurrentIteration { get; set; }
        public ConcurrentQueue<string> VerboseMessages { get; private set; }

        public FileCopy()
        {
            this.Errors = new ConcurrentQueue<Exception>();
            this.ShouldExitCurrentIteration = false;
            this.VerboseMessages = new ConcurrentQueue<string>();
        }

        public async Task<FileCopyResult> CopySubdirectoriesAsync(string inputDirectoryPath, string fileSearchPattern, int threads, string outputDirectoryPath)
        {
            FileCopyResult result = new FileCopyResult();

            IEnumerator<string> inputSubdirectories = Directory.EnumerateDirectories(inputDirectoryPath).GetEnumerator();
            if (inputSubdirectories.MoveNext() == false)
            {
                // no subdirectories to copy
                return result;
            }
            this.EnsureDirectoryExists(outputDirectoryPath);

            DirectoryInfo currentSubdirectory = new DirectoryInfo(inputSubdirectories.Current);
            string outputSubdirectoryPath = Path.Combine(outputDirectoryPath, currentSubdirectory.Name);
            this.EnsureDirectoryExists(outputSubdirectoryPath);
            IEnumerator<FileInfo> inputFiles = currentSubdirectory.EnumerateFiles(fileSearchPattern).GetEnumerator();

            int readWritePairs = (int)(0.5 * threads);
            long bytesReadInSubdirectory = 0;
            int filesInSubdirectory = 0;
            BlockingCollection<FileBuffer> files = new BlockingCollection<FileBuffer>(FileCopy.MaximumFilesAllocated - 1);
            List<Task> readers = new List<Task>(readWritePairs);
            List<Task> writers = new List<Task>(readWritePairs);
            for (int readWriteTaskPair = 0; readWriteTaskPair < readWritePairs; ++readWriteTaskPair)
            {
                ConcurrentBag<FileBuffer> availableBuffers = new ConcurrentBag<FileBuffer>();

                // read contents of input files
                readers.Add(Task.Run(() =>
                {
                    int bytesReadFromFile = 0;
                    FileCopyResult threadResult = new FileCopyResult();
                    while (this.ShouldExitCurrentIteration == false)
                    {
                        FileInfo inputFile;
                        lock (inputDirectoryPath)
                        {
                            bytesReadInSubdirectory += bytesReadFromFile;

                            // a while loop is needed to guarantee inputFiles.Current is set
                            // A second call to MoveNext() is needed when inputFiles is reassigned due to advancing to a new 
                            // subdirectory.
                            while (inputFiles.MoveNext() == false)
                            {
                                if (inputSubdirectories.MoveNext() == false)
                                {
                                    // no more files to read
                                    result.AccumulateThreadSafe(threadResult);
                                    return;
                                }

                                this.VerboseMessages.Enqueue(this.GetSubdirectoryCompleteMessage(currentSubdirectory.Name, filesInSubdirectory, bytesReadInSubdirectory));

                                bytesReadInSubdirectory = 0;
                                currentSubdirectory = new DirectoryInfo(inputSubdirectories.Current);
                                filesInSubdirectory = 0;
                                inputFiles = currentSubdirectory.EnumerateFiles(fileSearchPattern).GetEnumerator();
                                outputSubdirectoryPath = Path.Combine(outputDirectoryPath, currentSubdirectory.Name);
                                this.EnsureDirectoryExists(outputSubdirectoryPath);
                            }

                            inputFile = inputFiles.Current;
                            ++filesInSubdirectory;
                        }

                        bytesReadFromFile = 0;
                        FileInfo outputFile = new FileInfo(Path.Combine(outputSubdirectoryPath, inputFile.Name));
                        if (this.FileAlreadyCopied(inputFile, outputFile))
                        {
                            ++threadResult.FilesProcessed;
                            continue;
                        }
                        if (inputFile.Length == 0)
                        {
                            this.CreateEmptyFile(outputFile);
                            continue;
                        }

                        using (FileStream readStream = this.OpenForRead(inputFile.FullName))
                        {
                            if (availableBuffers.TryTake(out FileBuffer buffer) == false)
                            {
                                buffer = new FileBuffer();
                            }
                            try
                            {
                                while (bytesReadFromFile < FileCopy.BufferSizeLimit)
                                {
                                    if ((bytesReadFromFile + FileCopy.ReadBlockSize) > buffer.Bytes.Length)
                                    {
                                        buffer.Double();
                                    }
                                    int bytesRead = readStream.Read(buffer.Bytes, bytesReadFromFile, FileCopy.ReadBlockSize);
                                    bytesReadFromFile += bytesRead;
                                    if (bytesRead == 0)
                                    {
                                        if (bytesReadFromFile != inputFile.Length)
                                        {
                                            throw new IOException(String.Format("Expected to read {0} bytes from {1} but read {2}.", inputFile.Length, inputFile.FullName, bytesReadFromFile));
                                        }
                                        buffer.FileLength = bytesReadFromFile;
                                        buffer.FilePath = outputFile.FullName;
                                        files.Add(buffer);
                                        break;
                                    }
                                }
                            }
                            catch (IOException ioException)
                            {
                                this.Errors.Enqueue(new FileLoadException("Fatal error reading '" + inputFile.FullName + "'.  File skipped.", ioException));
                            }
                        }
                        threadResult.Accumulate(inputFile);
                    }
                }));

                // write contents of input files
                writers.Add(Task.Run(() =>
                {
                    while (files.IsCompleted == false)
                    {
                        if (files.TryTake(out FileBuffer file, Constant.ChunkRetryInterval) == false)
                        {
                            continue;
                        }
                        using (FileStream writeStream = this.OpenForWrite(file.FilePath))
                        {
                            for (int offset = 0; offset < file.FileLength; offset += FileCopy.WriteBlockSize)
                            {
                                int bytesToWrite = FileCopy.WriteBlockSize;
                                if ((offset + bytesToWrite) > file.FileLength)
                                {
                                    bytesToWrite = file.FileLength - offset;
                                }
                                writeStream.Write(file.Bytes, offset, bytesToWrite);
                            }
                        }
                        availableBuffers.Add(file);
                    }
                }));
            }

            await Task.WhenAll(readers);
            files.CompleteAdding();
            await Task.WhenAll(writers);
            this.VerboseMessages.Enqueue(this.GetSubdirectoryCompleteMessage(currentSubdirectory.Name, filesInSubdirectory, bytesReadInSubdirectory));
            return result;
        }

        private void CreateEmptyFile(FileInfo outputFile)
        {
            outputFile.OpenWrite().Dispose();
        }

        private void EnsureDirectoryExists(string outputDirectoryPath)
        {
            if (Directory.Exists(outputDirectoryPath) == false)
            {
                Directory.CreateDirectory(outputDirectoryPath);
            }
        }

        private bool FileAlreadyCopied(FileInfo inputFile, FileInfo outputFile)
        {
            return outputFile.Exists && (outputFile.Length == inputFile.Length);
        }

        private string GetSubdirectoryCompleteMessage(string subdirectoryName, int filesCopied, long bytesCopied)
        {
            string message = subdirectoryName + ": " + filesCopied + " files processed, ";
            if (filesCopied == 1)
            {
                message = subdirectoryName + " 1 file processed, ";
            }

            double gigabytesCopiedFromSubdirectory = bytesCopied / (1024.0 * 1024.0 * 1024.0);
            if (gigabytesCopiedFromSubdirectory >= 0.005)
            {
                return message + gigabytesCopiedFromSubdirectory.ToString("0.00") + " GB copied";
            }

            double megabytesCopiedFromSubdirectory = bytesCopied / (1024.0 * 1024.0);
            return message + megabytesCopiedFromSubdirectory.ToString("0.00") + " MB copied";
        }

        private FileStream OpenForRead(string filePath)
        {
            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, FileCopy.DefaultBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        }

        private FileStream OpenForWrite(string filePath)
        {
            return new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, FileCopy.DefaultBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan | FileOptions.WriteThrough);
        }

        private class FileBuffer
        {
            public byte[] Bytes { get; private set; }
            public int FileLength { get; set; }
            public string FilePath { get; set; }

            public FileBuffer()
            {
                this.Bytes = new byte[FileCopy.DefaultBufferSize];
                this.FileLength = 0;
                this.FilePath = null;
            }

            public void Double()
            {
                int newBufferSize = 2 * this.Bytes.Length;
                if (newBufferSize > FileCopy.BufferSizeLimit)
                {
                    throw new NotSupportedException(String.Format("Can't increase buffer to {0:.00}MB.", newBufferSize / (1024.0 * 1024.0)));
                }

                byte[] newBytes = new byte[newBufferSize];
                Buffer.BlockCopy(this.Bytes, 0, newBytes, 0, this.Bytes.Length);
                this.Bytes = newBytes;
            }
        }
    }
}
