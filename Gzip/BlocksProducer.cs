using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileManagerLibrary.Abstractions;
using DataCollection;
using System.Threading;

namespace Gzip
{
    public class BlocksProducer
    {
        private int QUEUE_WAIT_TRYADD_TIMEOUT = 100;
        private readonly IFileReader fileReader;
        private BlockingQueue<DataBlock> dataQueue;
        private bool stop = false;
        private Thread producingThread;
        private delegate void EventHandler(object sender, string message);
        private event EventHandler ErrorOccured;
        public BlocksProducer(IFileReader filereader, BlockingQueue<DataBlock> dataQueue)
        {
            this.fileReader = filereader;
            this.dataQueue = dataQueue;
        }

        private void Start()
        {
            producingThread = new Thread(new ThreadStart(ThreadWork));
            producingThread.Start();
        }

        public void Stop()
        {
            stop = true;
            producingThread.Join();
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
                            while (!dataQueue.CanAdd.WaitOne(QUEUE_WAIT_TRYADD_TIMEOUT))
                                if (stop)
                                {
                                    return;
                                }
                        }

                    }
                    dataQueue.IsCompleted = true;
                }
            }
            catch(Exception ex)
            {
                OnErrorOccured(this, ex.Message);
            }
        }
        private void OnErrorOccured(object sender,string message)
        {
            ErrorOccured?.Invoke(sender, message);
        }
      
    }
}
