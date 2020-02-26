using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileManagerLibrary.Abstractions;
using DataCollection;
using System.Threading;
using ExceptionsHandling;

namespace Gzip
{
    public class BlocksProducer : IErrorHandler, IStopProcess
    {
        private const int QUEUE_WAIT_TRYADD_TIMEOUT = 100;
        private readonly IFileReader fileReader;
        private BlockingQueue<DataBlock> dataQueue;
        private bool stop = false;
        private Thread producingThread;
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
                            while (!dataQueue.CanAdd.WaitOne(QUEUE_WAIT_TRYADD_TIMEOUT))
                                if (stop)
                                {
                                    return;
                                }
                        }
                        ExceptionsHandler.Log($"Blocksproducer added block {block.Index} to queue /n");
                        Console.WriteLine("Hello");
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
