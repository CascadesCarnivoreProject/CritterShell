using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CritterShell.Images
{
    internal class FileCopy
    {
        private const int BufferSize = 1024 * 1024;
        private const int MaximumChunksAllocated = 10;

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

        private FileStream OpenForRead(string filePath)
        {
            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, FileCopy.BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        }

        private FileStream OpenForWrite(string filePath)
        {
            return new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, FileCopy.BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        }

        public FileCopyResult Sequential(IEnumerable<FileInfo> inputFiles, string outputDirectoryPath, CancellationToken cancellationToken)
        {
            this.EnsureDirectoryExists(outputDirectoryPath);

            FileCopyResult result = new FileCopyResult();
            foreach (FileInfo inputFile in inputFiles)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return result;
                }

                FileInfo outputFile = new FileInfo(Path.Combine(outputDirectoryPath, inputFile.Name));
                if (this.FileAlreadyCopied(inputFile, outputFile))
                {
                    ++result.FilesProcessed;
                    continue;
                }

                File.Copy(inputFile.FullName, outputFile.FullName);
                result.Accumulate(inputFile);
            }

            return result;
        }

        public async Task<FileCopyResult> StreamAsync(IList<FileInfo> inputFiles, string outputDirectoryPath, CancellationToken cancellationToken)
        {
            // if no chunks are ever added to readChunks the call to Take() in the write task will throw
            FileCopyResult result = new FileCopyResult();
            if (inputFiles.Count < 1)
            {
                return result;
            }
            bool atLeastOneFileNotEmpty = false;
            foreach (FileInfo inputFile in inputFiles)
            {
                if (inputFile.Length > 0)
                {
                    atLeastOneFileNotEmpty = true;
                    break;
                }
            }
            if (atLeastOneFileNotEmpty == false)
            {
                throw new NotSupportedException("All input files are empty.");
            }
            this.EnsureDirectoryExists(outputDirectoryPath);

            // read contents of input files
            ConcurrentBag<FileChunk> availableChunks = new ConcurrentBag<FileChunk>();
            BlockingCollection<FileChunk> readChunks = new BlockingCollection<FileChunk>(FileCopy.MaximumChunksAllocated - 1);
            bool readCompleted = false;
            Task read = Task.Run(async () =>
            {
                foreach (FileInfo inputFile in inputFiles)
                {
                    if (inputFile == null)
                    {
                        continue;
                    }

                    FileInfo outputFile = new FileInfo(Path.Combine(outputDirectoryPath, inputFile.Name));
                    if (this.FileAlreadyCopied(inputFile, outputFile))
                    {
                        ++result.FilesProcessed;
                        continue;
                    }
                    if (inputFile.Length == 0)
                    {
                        this.CreateEmptyFile(outputFile);
                        continue;
                    }

                    using (FileStream readStream = this.OpenForRead(inputFile.FullName))
                    {
                        int bytesRead = 1;
                        while (bytesRead > 0)
                        {
                            if (availableChunks.TryTake(out FileChunk chunkToBeWritten) == false)
                            {
                                chunkToBeWritten = new FileChunk(FileCopy.BufferSize);
                            }
                            bytesRead = await readStream.ReadAsync(chunkToBeWritten.Bytes, 0, chunkToBeWritten.Bytes.Length);
                            if (bytesRead > 0)
                            {
                                chunkToBeWritten.BytesInUse = bytesRead;
                                chunkToBeWritten.FilePath = outputFile.FullName;
                                readChunks.Add(chunkToBeWritten);
                            }
                            else
                            {
                                availableChunks.Add(chunkToBeWritten);
                            }
                        }
                    }

                    result.Accumulate(inputFile);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        readChunks.CompleteAdding();
                        readCompleted = true;
                        return;
                    }
                }

                readChunks.CompleteAdding();
                readCompleted = true;
            });

            // write contents of input files
            Task write = Task.Run(async () =>
            {
                FileStream writeStream = null;
                try
                {
                    while ((readCompleted == false) || (readChunks.Count > 0))
                    {
                        FileChunk writeChunk = readChunks.Take();
                        if (writeStream == null)
                        {
                            writeStream = this.OpenForWrite(writeChunk.FilePath);
                        }
                        else if (String.Equals(writeStream.Name, writeChunk.FilePath, StringComparison.OrdinalIgnoreCase) == false)
                        {
                            writeStream.Dispose();
                            writeStream = this.OpenForWrite(writeChunk.FilePath);
                        }
                        await writeStream.WriteAsync(writeChunk.Bytes, 0, writeChunk.BytesInUse);
                    }
                }
                finally
                {
                    if (writeStream != null)
                    {
                        writeStream.Dispose();
                    }
                }
            });

            await read;
            await write;
            return result;
        }

        private struct FileChunk
        {
            public byte[] Bytes { get; private set; }
            public int BytesInUse { get; set; }
            public string FilePath { get; set; }

            public FileChunk(int size)
            {
                this.Bytes = new byte[size];
                this.BytesInUse = 0;
                this.FilePath = null;
            }
        }
    }
}
