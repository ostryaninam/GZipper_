using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using ExceptionsHandling;
using FileManagerLibrary;
using FileManagerLibrary.Abstractions;
using FileManagerLibrary.Implementations;
using FixedThreadPool;
using DataCollection;

namespace Gzip
{
    public class GZipDecompressor: GZipper
    {
        string pathFrom;
        string pathTo;
        FixedThreadPool.FixedThreadPool threadPool;
        object fileReadLocker = new object();
        BlockingDictionary dataDictionary;
        long timeSummaryGZip = 0;
        long timeSummaryWrite = 0;
        object sumLocker = new object();
        object sumWriteLocker = new object();
        
        public GZipDecompressor(string pathFrom,string pathTo)
        {
            this.pathFrom = pathFrom;
            this.pathTo = pathTo;
            threadPool = FixedThreadPool.FixedThreadPool.GetInstance();
            dataDictionary = new BlockingDictionary();
            readyBlockEvent = new AutoResetEvent(false);
            canWrite = new ManualResetEvent(false);
            endSignal = new CountdownEvent(threadPool.Count);
        }
        public override void DoGZipWork()
        {
            using (fileFrom = new CompressedFileFactory(pathFrom).GetFileReader())
                using (fileTo = new SimpleFileFactory(pathTo, 1024 * 1024).GetFileWriter())
            {
                StartThreads();
                endSignal.Wait();
                Console.WriteLine("Успешно");
            }
        }
        private void StartThreads()
        {
            for (int i = 0; i < threadPool.Count - 1; i++)
                threadPool.Execute(() => GzipThreadWork());
            threadPool.Execute(() => WritingThreadWork());
        }
        protected override byte[] GZipOperation(byte[] inputBytes)
        {
            return DecompressBlock(inputBytes);
        }

        byte[] DecompressBlock(byte[] bytesToDecompress)
        {
            byte[] resultBytes = null;
            try
            {
                using (MemoryStream streamFrom = new MemoryStream(bytesToDecompress))
                {
                    using (MemoryStream streamTo = new MemoryStream())
                    {
                        using (GZipStream gzipStream = new
                            GZipStream(streamFrom, CompressionMode.Decompress))
                        {
                            gzipStream.CopyTo(streamTo);
                            gzipStream.Flush();
                        }
                        resultBytes = streamTo.ToArray();
                        streamTo.Flush();
                    }
                }

                return resultBytes;
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected override void GzipThreadWork()
        {
            while (!fileFrom.EndOfFile)
            {
                Stopwatch stopwatch = new Stopwatch();
                DataBlock result = null;
                DataBlock fileBlock = null;
                stopwatch.Start();
                try
                {
                    lock (fileReadLocker)
                    {
                        fileBlock = fileFrom.ReadBlock();
                    }
                    result = new DataBlock(fileBlock.Index, GZipOperation(fileBlock.GetBlockBytes));
                }
                catch (Exception e)
                {
                    ExceptionsHandler.Handle(this.GetType(), e);
                }
                dataDictionary.TryAdd(result.Index,result.GetBlockBytes);

            }
            endSignal.Signal();
        }
        protected override void WritingThreadWork()
        {
            long writtenBlocks = 0;
            for (int i=0; i< fileFrom.NumberOfBlocks; i++)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                dataDictionary.TryTake(i, out var block);                
                fileTo.WriteBlock(new DataBlock(i,block));
                writtenBlocks++;
               
                stopwatch.Stop();
                lock (sumWriteLocker)
                    timeSummaryWrite += stopwatch.ElapsedMilliseconds;
                ExceptionsHandler.Log($"Writing thread " +
                    $"wrote block number {i} in {stopwatch.ElapsedMilliseconds} ms");
            }
            ExceptionsHandler.Log($"Time on writing: {timeSummaryWrite}");
            endSignal.Signal();
        }

    }
}
