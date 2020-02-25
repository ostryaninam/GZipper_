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
        private int BLOCK_SIZE = 1024 * 1024;
        private FixedThreadPool.FixedThreadPool threadPool;
        private string pathFrom;
        private string pathTo;
        private BlockingQueue<DataBlock> inputQueue;
        private BlockingQueue<DataBlock> outputQueue;
        public GZipCompressor(string pathFromName ,string pathToName)
        {
            this.pathFrom = pathFromName;   //TODO когда писать this
            this.pathTo = pathToName;
            this.threadPool = FixedThreadPool.FixedThreadPool.GetInstance(); 
            this.outputQueue = new BlockingQueue<DataBlock>();
            this.inputQueue = new BlockingQueue<DataBlock>();
        }
        public override void Start()
        {
            BlocksProducer blocksProducer = new BlocksProducer(
                new SimpleFileFactory(pathFrom, BLOCK_SIZE).GetFileReader(),
                this.outputQueue);
            BlocksConsumer blocksConsumer = new BlocksConsumer(
                new CompressedFileFactory(pathTo).GetFileWriter(),
                this.outputQueue);
            blocksProducer.Start();
            blocksConsumer.Start();
            GzipWork();
        }

        private void StartThread()
        {

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
            while(!(inputQueue.IsCompleted && inputQueue.IsEmpty))
            {
                if (!threadPool.IsStopping)
                {
                    threadPool.Execute(() =>
                    {
                        if (inputQueue.TryTake(out var dataBlock))
                        {
                            var result = new DataBlock(dataBlock.Index, GZipOperation(dataBlock.GetBlockBytes));
                            if (!outputQueue.TryAdd(result))
                                outputQueue.CanAdd.WaitOne();
                        }
                        else
                            outputQueue.CanTake.WaitOne();
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
