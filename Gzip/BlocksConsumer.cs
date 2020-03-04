using DataCollection;
using FileManagerLibrary.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Gzip
{
    public class BlocksConsumer : IErrorHandler, IThread, IEnding
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private const int QUEUE_WAIT_TRYADD_TIMEOUT = 100;
        private readonly IFileWriter fileWriter;
        private readonly BlockingQueue<DataBlock> dataQueue;
        private bool stop = false;
        private Thread consumingThread;

        public event ErrorHandler ErrorOccured;
        public event EndHasCome EndEvent;

        public BlocksConsumer(IFileWriter fileWriter, BlockingQueue<DataBlock> dataQueue)
        {
            this.fileWriter = fileWriter;
            this.dataQueue = dataQueue;
        }
        public void Start()
        {
            consumingThread = new Thread(new ThreadStart(ThreadWork));
            consumingThread.Start();
        }
        public void Stop()
        {
            stop = true;
            if (consumingThread.IsAlive)
                consumingThread.Join();
            fileWriter.Dispose();
        }
        public void ThreadWork()
        {
            try
            {
                using (fileWriter)
                {
                    while (!(dataQueue.IsEmpty && dataQueue.IsCompleted))
                    {
                        if (stop)
                        {
                            return;
                        }
                        DataBlock block = null;
                        while (!dataQueue.TryTake(out block))
                        {
                            while (!dataQueue.CanTake.WaitOne(QUEUE_WAIT_TRYADD_TIMEOUT))
                                if (stop)
                                {
                                    return;
                                }
                        }
                        fileWriter.WriteBlock(block);
                        logger.Info($"Blocksconsumer wrote block {block.Index}");
                    }
                }
            }
            catch (Exception ex)
            {
                OnErrorOccured(ex);
            }
        }
        private void OnErrorOccured(Exception ex)
        {
            this.ErrorOccured?.Invoke(this, ex);
        }

        private void OnEnded()
        {
            this.EndEvent?.Invoke(this);
        }
    }
}
