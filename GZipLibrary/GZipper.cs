using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataCollection;
using NLog;

namespace GZipLibrary
{
    class GZipper : IWorker
    {
        private bool EndCondition => inputQueue.IsCompleted && inputQueue.IsEmpty;

        protected static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected const int WAIT_FOR_BLOCK_TIMEOUT = 100;
        protected const int WAIT_FOR_ADD_BLOCK_TIMEOUT = 100;

        protected readonly BlockingQueue inputQueue;
        protected readonly IBlockingCollection outputQueue;
        protected readonly IBlockGZipper blockGZipper;
        protected readonly int numOfThreads;
        protected bool isStopping = false;
        protected CountdownEvent threadsCompleted;

        public event EventHandler<Exception> ErrorOccured;
        public event EventHandler CompleteEvent;
        public GZipper(BlockingQueue inputQueue, IBlockingCollection outputQueue, 
            IBlockGZipper blockGZipper, int numOfThreads)
        {
            this.inputQueue = inputQueue;
            this.outputQueue = outputQueue;
            this.blockGZipper = blockGZipper;
            this.numOfThreads = numOfThreads;
        }

        public void Start()
        {
            this.threadsCompleted = new CountdownEvent(numOfThreads);
            for (int i = 0; i < this.numOfThreads; i++)
            {
                var gzipThread = new Thread(new ThreadStart(() => ThreadWork()));
                gzipThread.Start();
            }
            CompleteEvent += (sender, e) =>
            {
                outputQueue.IsCompleted = true;
                logger.Debug("Queue is marked as completed");
            };
        }
        public void Stop()
        {
            isStopping = true;
            threadsCompleted.Wait();
            OnCompleted();
        }
        private void ThreadWork()
        {
            try
            {
                while (!EndCondition)
                {
                    if (isStopping)
                    {
                        Complete();
                        return;
                    }
                    long numOfBlock = 0;
                    if (inputQueue.TryTake(out var dataBlock))
                    {
                        var result = new DataBlock(dataBlock.Index, blockGZipper.Execute(dataBlock.BlockBytes));
                        numOfBlock = result.Index;
                        while (!outputQueue.TryAdd(result))
                        {
                            logger.Debug($"Thread number {Thread.CurrentThread.ManagedThreadId} " +
                            $"trying add block {numOfBlock} to queue");
                            while (!outputQueue.CanAdd.WaitOne(WAIT_FOR_ADD_BLOCK_TIMEOUT))
                                if (isStopping||EndCondition)
                                {
                                    Complete();
                                    return;
                                }
                        }
                        logger.Debug($"Thread number {Thread.CurrentThread.ManagedThreadId} " +
                            $"added gzipped block {numOfBlock} to queue");
                    }
                    else
                    {
                        logger.Debug($"Thread {Thread.CurrentThread.ManagedThreadId} " +
                            $"waiting for cantake signal");
                        outputQueue.CanTake.WaitOne(WAIT_FOR_BLOCK_TIMEOUT);
                    }
                }
                Complete();
            }
            catch (Exception ex)
            {
                OnErrorOccured(ex);
            }
           
        }
        private void Complete()
        {
            logger.Debug($"Thread number {Thread.CurrentThread.ManagedThreadId} " +
                            $"ended working");
            threadsCompleted.Signal();
            if (threadsCompleted.CurrentCount == 0)
                OnCompleted();
        }
        private void OnErrorOccured(Exception ex)
        {
            this.ErrorOccured?.Invoke(this, ex);
        }

        private void OnCompleted()
        {
            this.CompleteEvent?.Invoke(this, new EventArgs());
        }
    }
}
