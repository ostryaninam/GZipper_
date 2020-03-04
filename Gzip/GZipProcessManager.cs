using DataCollection;
using FileManagerLibrary.Implementations;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gzip
{
    class GZipProcessManager : IProcessManager
    {
        private static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private const int BLOCK_SIZE = 1024 * 1024;

        private readonly string pathFrom;
        private readonly string pathTo;
        private readonly BlockingQueue<DataBlock> inputQueue;
        private readonly BlockingQueue<DataBlock> outputQueue;
        private readonly IBlockGZipper blockGZipper;

        private readonly BlocksConsumer blocksConsumer;
        private readonly BlocksProducer blocksProducer;
        private readonly List<IThread> allThreads;
        private bool isStopping = false;

        public AutoResetEvent End { get; }
        public Exception Exception { get; private set; }

        //private CountdownEvent threadsFinished;

        public GZipProcessManager(string pathFromName, string pathToName, GZipOperation operation)
        {
            this.pathFrom = pathFromName;  
            this.pathTo = pathToName;
            this.outputQueue = new BlockingQueue<DataBlock>();
            this.inputQueue = new BlockingQueue<DataBlock>();
            this.blocksProducer = new BlocksProducer(
                new SimpleFileFactory(pathFrom, BLOCK_SIZE).GetFileReader(),
                this.inputQueue);
            this.blocksConsumer = new BlocksConsumer(
                new CompressedFileFactory(pathTo).GetFileWriter(),
                this.outputQueue);
            this.allThreads = new List<IThread>();
            if (operation == GZipOperation.Compress)
                this.blockGZipper = new BlockGZipCompressor();
            else
                this.blockGZipper = new BlockGZipDecompressor();
            this.End = new AutoResetEvent(false);
        }
        public void StartProcess()
        {
            ((IEnding)blocksConsumer).EndEvent += (sender) => End.Set();
            InitializateThreads();        
            foreach (var thread in allThreads)
            {
                ((IErrorHandler)thread).ErrorOccured += ErrorHandling;
            }
            StartThreads();
        }

        private void InitializateThreads()
        {
            this.allThreads.Add(blocksProducer);
            var gzipThreadsCount = 1;
            if (Environment.ProcessorCount > 2)
                gzipThreadsCount = Environment.ProcessorCount - 2;
            //this.threadsFinished = new CountdownEvent(threadsCount);
            for (int i = 0; i < gzipThreadsCount; i++)
            {
                this.allThreads.Add(new GZipperThread(inputQueue, outputQueue, blockGZipper));
            }
            allThreads.Add(blocksConsumer);
        }
        private void StartThreads()
        {
            foreach (var thread in allThreads)
                thread.Start();
            //outputQueue.IsCompleted = true; TODO how to...
        }
        
        private void StopAll()
        {
            Thread stoppingThread = new Thread(new ThreadStart(() =>
            {
                foreach (var thread in allThreads)
                {
                    thread.Stop();
                }
            }));
            stoppingThread.Start();
            stoppingThread.Join();
        }
        private void ErrorHandling(object sender, Exception ex)
        {
            StopAll();
            this.Exception = ex;
            this.End.Set();
        }

    }
}
