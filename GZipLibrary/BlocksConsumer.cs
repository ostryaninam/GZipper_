using DataCollection;
using FileManagerLibrary.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace GZipLibrary
{
    abstract class BlocksConsumer : IWorker
    {
        protected static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

        protected const int QUEUE_WAIT_TRYADD_TIMEOUT = 100;
        protected readonly IFileWriter fileWriter;
        protected readonly IBlockingCollection dataCollection;
        protected bool stop = false;
        protected Thread consumingThread;

        public event EventHandler<Exception> ErrorOccured;
        public event EventHandler CompleteEvent;

        public BlocksConsumer(IFileWriter fileWriter, IBlockingCollection dataQueue)
        {
            this.fileWriter = fileWriter;
            this.dataCollection = dataQueue;
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
        public abstract void ThreadWork();
        protected void OnErrorOccured(Exception ex)
        {
            this.ErrorOccured?.Invoke(this, ex);
        }

        protected void OnCompleted()
        {
            this.CompleteEvent?.Invoke(this, new EventArgs());
        }
    }
}
