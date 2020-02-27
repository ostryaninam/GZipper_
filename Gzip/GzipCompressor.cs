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
    public class GZipCompressor : GZipper, IErrorHandler, IStopProcess
    {
        private const int BLOCK_SIZE = 1024 * 1024;
        private const int WAIT_FOR_BLOCK_TIMEOUT = 500;
        private const int WAIT_FOR_ADD_BLOCK_TIMEOUT = 300;

        private string pathFrom;
        private string pathTo;
        private bool isStopping = false;
        private BlockingQueue<DataBlock> inputQueue;
        private BlockingQueue<DataBlock> outputQueue;
        private CountdownEvent threadsFinished;
        private delegate void EventHandler(object sender, string message);
        private List<IErrorHandler> errorHandlers;
        public event ErrorHandler ErrorOccured;
        public GZipCompressor(string pathFromName ,string pathToName)
        {
            this.pathFrom = pathFromName;   //TODO когда писать this
            this.pathTo = pathToName;   
            this.outputQueue = new BlockingQueue<DataBlock>();
            this.inputQueue = new BlockingQueue<DataBlock>();
            this.errorHandlers = new List<IErrorHandler>();
        }
        public override void Start()
        {
            BlocksProducer blocksProducer = new BlocksProducer(
                new SimpleFileFactory(pathFrom, BLOCK_SIZE).GetFileReader(),
                this.inputQueue);
            BlocksConsumer blocksConsumer = new BlocksConsumer(
                new CompressedFileFactory(pathTo).GetFileWriter(),
                this.outputQueue);
            this.errorHandlers.Add(blocksProducer);
            this.errorHandlers.Add(blocksConsumer);
            this.errorHandlers.Add(this);
            foreach (var handler in errorHandlers)
                handler.ErrorOccured += ErrorHandling;
            blocksProducer.Start();
            blocksConsumer.Start();
            StartThreads();
        }
        public void Stop()
        {
            ExceptionsHandler.Log("Trying to stop compressor");
            isStopping = true;
            threadsFinished.Wait();
        }
        private void StartThreads()
        {            
            var threadsCount = 1;
            if (Environment.ProcessorCount > 2)
                threadsCount = Environment.ProcessorCount - 2;
            this.threadsFinished = new CountdownEvent(threadsCount);
            Thread[] threads = new Thread[threadsCount];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(new ThreadStart(ThreadWork));
                threads[i].Start();
            }
            threadsFinished.Wait();
            outputQueue.IsCompleted = true;
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
        protected override void ThreadWork()
        {
            try
            {
                while (!(inputQueue.IsCompleted && inputQueue.IsEmpty))
                {
                    if (isStopping)
                    {
                        threadsFinished.Signal();
                        return;
                    }
                    long numOfBlock = 0;
                    if (inputQueue.TryTake(out var dataBlock))
                    {
                        var result = new DataBlock(dataBlock.Index, GZipOperation(dataBlock.GetBlockBytes));
                        numOfBlock = result.Index;
                        while (!outputQueue.TryAdd(result))
                        {
                            ExceptionsHandler.Log($"Thread number {Thread.CurrentThread.ManagedThreadId} " +
                            $"trying add block {numOfBlock} to queue");
                            while (!outputQueue.CanAdd.WaitOne(WAIT_FOR_ADD_BLOCK_TIMEOUT))
                                if (isStopping)
                                {
                                    threadsFinished.Signal();
                                    return;
                                }
                        }
                        ExceptionsHandler.Log($"Thread number {Thread.CurrentThread.ManagedThreadId} " +
                            $"added gzipped block {numOfBlock} to queue");
                    }
                    else
                    {
                        ExceptionsHandler.Log($"Thread {Thread.CurrentThread.ManagedThreadId} " +
                            $"waiting for cantake signal");
                        outputQueue.CanTake.WaitOne(WAIT_FOR_BLOCK_TIMEOUT);
                    }
                }
                threadsFinished.Signal();
            }
            catch (Exception ex)
            {
                OnErrorOccured(ex.Message);
            }

            ExceptionsHandler.Log($"Thread number {Thread.CurrentThread.ManagedThreadId} " +
                        $"ended working");
        }
        private void StopAll()
        {
            Thread stoppingThread = new Thread(new ThreadStart(() =>
            {
                foreach (var handler in errorHandlers)
                {
                    ((IStopProcess)handler).Stop();
                    ExceptionsHandler.Log("Stopped one of handlers");
                }
            }));
            stoppingThread.Start();
        }
        private void OnErrorOccured(string message)
        {
            ErrorOccured?.Invoke(this, message);
        }
        private void ErrorHandling(object sender, string message) //TODO доделать
        {
            ExceptionsHandler.Log($"Error in {sender}: {message}");
            StopAll();
        }

    }
}
