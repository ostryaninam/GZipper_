using DataCollection;
using FileManagerLibrary.Implementations;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public enum GZipOperation { Compress, Decompress };

namespace GZipLibrary
{
    public class GZipProcessManager : IProcessManager
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private const int BLOCK_SIZE = 1024 * 1024;

        private readonly string pathFrom;
        private readonly string pathTo;
        private readonly BlockingQueue producerQueue;
        private readonly IBlockingCollection consumerCollection;
        private readonly IBlockGZipper blockGZipper;

        private readonly BlocksConsumer blocksConsumer;
        private readonly BlocksProducer blocksProducer;
        private readonly List<IWorker> allWorkers;

        private AutoResetEvent End { get; }
        public Exception Exception { get; private set; }
        public GZipProcessManager(string pathFromName, string pathToName, GZipOperation operation)
        {
            this.pathFrom = pathFromName;  
            this.pathTo = pathToName;
            this.allWorkers = new List<IWorker>();
            this.producerQueue = new BlockingQueue();

            if (operation == GZipOperation.Compress)
            {
                this.consumerCollection = new BlockingQueue();
                this.blockGZipper = new BlockGZipCompressor();
                this.blocksProducer = new BlocksProducer(
                    new SimpleFileFactory(pathFrom, BLOCK_SIZE).GetFileReader(),
                    this.producerQueue);
                this.blocksConsumer = new CompressBlocksConsumer(
                    new CompressedFileFactory(pathTo).GetFileWriter(),
                    this.consumerCollection);
            }
            else
            {
                this.blockGZipper = new BlockGZipDecompressor();
                this.consumerCollection = new BlockingDictionary();
                this.blocksProducer = new BlocksProducer(
                    new SimpleFileFactory(pathFrom, BLOCK_SIZE).GetFileReader(),
                    this.producerQueue);
                this.blocksConsumer = new CompressBlocksConsumer(
                    new CompressedFileFactory(pathTo).GetFileWriter(),
                    this.consumerCollection);
            }

            this.End = new AutoResetEvent(false);
        }
        public AutoResetEvent StartProcess()
        {
            blocksConsumer.CompleteEvent += (sender, e) => End.Set();             
            InitializateWorkers();        
            foreach (var thread in allWorkers)
            {
                thread.ErrorOccured += ErrorHandling;
            }
            StartWorkers();
            return End;
        }
        private void InitializateWorkers()
        {
            this.allWorkers.Add(blocksProducer);
            var gzipThreadsCount = 1;
            if (Environment.ProcessorCount > 2)
                gzipThreadsCount = Environment.ProcessorCount - 2;
            this.allWorkers.Add(new GZipper(producerQueue, consumerCollection, blockGZipper, gzipThreadsCount));
            allWorkers.Add(blocksConsumer);
        }
        private void StartWorkers()
        {
            foreach (var thread in allWorkers)
                thread.Start();
        }        
        private void StopAll()
        {
            Thread stoppingThread = new Thread(new ThreadStart(() =>
            {
                foreach (var worker in allWorkers)
                {
                    worker.Stop();
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
