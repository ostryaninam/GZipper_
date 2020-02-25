using DataCollection;
using FileManagerLibrary.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gzip
{
    public class BlocksConsumer
    {
        private int QUEUE_WAIT_TRYADD_TIMEOUT = 100;
        private readonly IFileWriter fileWriter;
        private BlockingQueue<DataBlock> dataQueue;
        private bool stop = false;
        private Thread producingThread;
        private delegate void EventHandler(object sender, string message);
        private event EventHandler ErrorOccured;
        public int CountOfBlocks { get; set; }
        public BlocksConsumer(IFileWriter fileWriter, BlockingQueue<DataBlock> dataQueue)
        {
            this.fileWriter = fileWriter;
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
        }
        public void ThreadWork()
        {
            if (CountOfBlocks != 0)
            {
                try
                {
                    using (fileWriter)
                    {
                        int writtenBlocks = 0;
                        while (writtenBlocks <= CountOfBlocks)
                        {
                            if (stop)
                            {
                                return;
                            }
                            DataBlock block = null;
                            while(!dataQueue.TryTake(out block))
                            {
                                while(!dataQueue.CanTake.WaitOne(QUEUE_WAIT_TRYADD_TIMEOUT))
                                    if (stop)
                                    {
                                        return;
                                    }
                            }
                            fileWriter.WriteBlock(block);
                            writtenBlocks++;                                                      
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnErrorOccured(this, ex.Message);
                } 
            }
        }
        private void OnErrorOccured(object sender, string message)
        {
            ErrorOccured?.Invoke(sender, message);
        }
    }
}
