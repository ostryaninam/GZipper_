using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataCollection;
using NLog;

namespace Gzip
{
    class GZipper : IThread, IErrorHandler, ICompleted
    {
        private static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private const int WAIT_FOR_BLOCK_TIMEOUT = 100;
        private const int WAIT_FOR_ADD_BLOCK_TIMEOUT = 100;

        private readonly BlockingQueue<DataBlock> inputQueue;
        private readonly BlockingQueue<DataBlock> outputQueue;
        private readonly IBlockGZipper blockGZipper;
        private Thread workingThread;
        private bool isStopping = false;

        public event ErrorHandler ErrorOccured;
        public event CompleteEventHandler CompleteEvent;

        public GZipper(BlockingQueue<DataBlock> inputQueue, BlockingQueue<DataBlock> outputQueue, 
            IBlockGZipper blockGZipper)
        {
            this.inputQueue = inputQueue;
            this.outputQueue = outputQueue;
            this.blockGZipper = blockGZipper;
        }

        public void Start()
        {
            workingThread = new Thread(new ThreadStart(() => ThreadWork()));
            workingThread.Start();
        }
        public void Stop()
        {
            isStopping = true;
            workingThread.Join();
        }

        protected void ThreadWork()
        {
            try
            {
                while (!(inputQueue.IsCompleted && inputQueue.IsEmpty))
                {
                    if (isStopping)
                    {
                        //threadsFinished.Signal();
                        return;
                    }
                    long numOfBlock = 0;
                    if (inputQueue.TryTake(out var dataBlock))
                    {
                        var result = new DataBlock(dataBlock.Index, blockGZipper.Execute(dataBlock.BlockBytes));
                        numOfBlock = result.Index;
                        while (!outputQueue.TryAdd(result))
                        {
                            logger.Info($"Thread number {Thread.CurrentThread.ManagedThreadId} " +
                            $"trying add block {numOfBlock} to queue");
                            while (!outputQueue.CanAdd.WaitOne(WAIT_FOR_ADD_BLOCK_TIMEOUT))
                                if (isStopping)
                                {
                                    //threadsFinished.Signal();
                                    return;
                                }
                        }
                        logger.Info($"Thread number {Thread.CurrentThread.ManagedThreadId} " +
                            $"added gzipped block {numOfBlock} to queue");
                    }
                    else
                    {
                        logger.Info($"Thread {Thread.CurrentThread.ManagedThreadId} " +
                            $"waiting for cantake signal");
                        outputQueue.CanTake.WaitOne(WAIT_FOR_BLOCK_TIMEOUT);
                    }
                }
                //threadsFinished.Signal();
            }
            catch (Exception ex)
            {
                OnErrorOccured(ex);
            }

            logger.Info($"Thread number {Thread.CurrentThread.ManagedThreadId} " +
                        $"ended working");
        }
        private void OnErrorOccured(Exception ex)
        {
            this.ErrorOccured?.Invoke(this, ex);
        }

        private void OnCompleted()
        {
            this.CompleteEvent?.Invoke(this);
        }
    }
}
