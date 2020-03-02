using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileManagerLibrary.Abstractions;
using DataCollection;
using System.Threading;
using NLog;

namespace Gzip
{
    public class BlocksProducer : IErrorHandler, IStopProcess
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private const int QUEUE_WAIT_TRYADD_TIMEOUT = 100;
        private readonly IFileReader fileReader;
        private BlockingQueue<DataBlock> dataQueue;
        private bool stop = false;
        public Thread producingThread;
        public event ErrorHandler ErrorOccured;
        public BlocksProducer(IFileReader filereader, BlockingQueue<DataBlock> dataQueue)
        {
            this.fileReader = filereader;
            this.dataQueue = dataQueue;
        }

        public void Start()
        {
            producingThread = new Thread(new ThreadStart(ThreadWork));
            producingThread.Start();
        }

        public void Stop()
        {
            stop = true;
            if (producingThread.IsAlive)
                producingThread.Join();
            fileReader.Dispose();
        }
        public void ThreadWork()
        {
            try
            {
                using (fileReader)
                {
                    foreach (var block in fileReader)
                    {
                        if (stop)
                        {
                            return;
                        }
                        while (!dataQueue.TryAdd(block))
                        {
                            logger.Info($"Blocksproducer trying to add block {block.Index} to queue");
                            while (!dataQueue.CanAdd.WaitOne(QUEUE_WAIT_TRYADD_TIMEOUT))
                            {
                                logger.Info($"Blocksproducer waiting for canadd signal" +
                                    $" {block.Index} to queue");
                                if (stop)
                                {
                                    return;
                                }
                            }
                        }
                        logger.Info($"Blocksproducer added block {block.Index} to queue");
                    }
                    dataQueue.IsCompleted = true;
                }
            }
            catch(Exception ex)
            {
                OnErrorOccured(ex);
            }
        }
        private void OnErrorOccured(Exception ex)
        {
            this.ErrorOccured?.Invoke(this, ex);
        }
      
    }
}
