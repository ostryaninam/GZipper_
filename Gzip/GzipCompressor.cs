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
        private FixedThreadPool.FixedThreadPool threadPool;
        private string pathFrom;
        private string pathTo;
        private BlockingQueue<DataBlock> consumingQueue;
        private BlockingQueue<DataBlock> producingQueue;
        private FileDispatcher fileDispatcher;
        public GZipCompressor(string pathFromName ,string pathToName)
        {
            pathFrom = pathFromName;
            pathTo = pathToName;
            threadPool = FixedThreadPool.FixedThreadPool.GetInstance(); 
            producingQueue = new BlockingQueue<DataBlock>();
            consumingQueue = new BlockingQueue<DataBlock>();
        }
        public override void DoGZipWork()
        {

            fileDispatcher = new FileDispatcher(new SimpleFileFactory(pathFrom, 1024 * 1024).GetFileReader(),
                new CompressedFileFactory(pathTo).GetFileWriter());
            fileDispatcher.ReadBlocks(consumingQueue);
            fileDispatcher.WriteBlocks(producingQueue);
            GzipWork();
        }

        protected override byte[] GZipOperation(byte[] inputBytes)
        {
            return CompressBlock(inputBytes);
        }
        private byte[] CompressBlock(byte[] bytesToCompress)
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
        protected override void GzipWork()
        {
            while(!(consumingQueue.IsCompleted && consumingQueue.IsEmpty))
            {
                if (!threadPool.IsStopping)
                {
                    threadPool.Execute(() =>
                    {
                        if (consumingQueue.TryTake(out var dataBlock))
                        {
                            var result = new DataBlock(dataBlock.Index, GZipOperation(dataBlock.GetBlockBytes));
                            if (!producingQueue.TryAdd(result))
                                producingQueue.ItemTaken.WaitOne();
                        }
                        else
                            producingQueue.ItemAdded.WaitOne();
                    });
                }
                else
                    break;
            }
        }
        //protected override void GzipThreadWork()
        //{
        //    while (!fileFrom.EndOfFile)
        //    {
        //        DataBlock fileBlock = null;
        //        DataBlock result = null;
        //        try
        //        {
        //            lock (fileReadLocker)
        //            {
        //                fileBlock = fileFrom.ReadBlock();
        //            }
        //            result = new DataBlock(fileBlock.Index, GZipOperation(fileBlock.GetBlockBytes));
        //        }
        //        catch (Exception e)
        //        {
        //            ExceptionsHandler.Handle(this.GetType(), e);
        //        }
        //        producingQueue.TryAdd(result);
        //    }
        //    endSignal.Signal();
        //}

    }
}
