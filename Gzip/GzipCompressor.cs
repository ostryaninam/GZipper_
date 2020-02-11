using FileManagerLibrary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileManagerLibrary.Abstractions;
using FileManagerLibrary.Implementations;
using DataCollection;
using ExceptionsHandling;
using System.Diagnostics;

namespace Gzip
{
    public class GZipCompressor : GZipper
    {
        FixedThreadPool.FixedThreadPool threadPool;
        string pathFrom;
        string pathTo;
        object fileReadLocker = new object();
        public GZipCompressor(string pathFrom,string pathTo)
        {
            this.pathFrom = pathFrom;
            this.pathTo = pathTo;
        }
        protected void DoGzipWork()
        {
            threadPool = FixedThreadPool.FixedThreadPool.GetInstance(); //TODO naming
            dataBlocks = new DataCollection.BlockingDataCollection();
            readyBlockEvent = new AutoResetEvent(false);
            canWrite = new ManualResetEvent(false);
            endSignal = new CountdownEvent(threadPool.Count);

            for (int i = 0; i < threadPool.Count - 1; i++)
                threadPool.Execute(() => GzipThreadWork());
            threadPool.Execute(() => WritingThreadWork());
            endSignal.Wait();
            Console.WriteLine("Успешно");
        }
      
        public void Compress()
        {
            using (fileFrom = new SimpleFileFactory(pathFrom, 1024 * 1024).GetFileReader())
                using(fileTo = new CompressedFileFactory(pathTo).GetFileWriter())

            {
                fileTo.WriteInt32(fileFrom.NumberOfBlocks);
                DoGzipWork();                    
            }
        }

        protected override byte[] GZipOperation(byte[] inputBytes)
        {
            return CompressBlock(inputBytes);
        }
        byte[] CompressBlock(byte[] bytesToCompress)
        {
            byte[] resultBytes = null;

            try
            {
                using (MemoryStream streamTo = new MemoryStream())
                {
                    using (GZipStream gzipStream = new
                        GZipStream(streamTo, CompressionMode.Compress))
                    {
                        gzipStream.Write(bytesToCompress, 0, bytesToCompress.Length);
                        gzipStream.Flush();
                    }
                    resultBytes = streamTo.ToArray();
                }

                return resultBytes;
            }
            catch (Exception)
            {
                throw;
            }

        }
        protected new void GzipThreadWork()
        {
            while (!fileFrom.EndOfFile)
            {
                Stopwatch stopwatch = new Stopwatch();

                DataBlock fileBlock = null;
                DataBlock result = null;
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
                dataBlocks.TryAdd(result);
                stopwatch.Stop();
                ExceptionsHandler.Log($"Gzip thread number {Thread.CurrentThread.ManagedThreadId} " +
                    $"gzipped block in {stopwatch.ElapsedMilliseconds} ms");
            }
            endSignal.Signal();
        }
        protected new void WritingThreadWork()
        {            
            long writtenBlocks = 0;
            while (fileFrom.NumberOfBlocks > writtenBlocks) //TODO could I do it better?
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                dataBlocks.TryTake(out var block);                
                fileTo.WriteBlock(block);
                writtenBlocks++;
                stopwatch.Stop();
                ExceptionsHandler.Log($"Writing thread " +
                    $"wrote block in {stopwatch.ElapsedMilliseconds} ms");
            }
            endSignal.Signal();
        }
    }
}
