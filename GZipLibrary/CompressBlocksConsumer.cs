using DataCollection;
using FileManagerLibrary.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace GZipLibrary
{
    class CompressBlocksConsumer : BlocksConsumer
    {
        public CompressBlocksConsumer(IFileWriter fileWriter, IBlockingCollection dataQueue) :
            base(fileWriter, dataQueue) { }
        public override void ThreadWork()
        {
            try
            {
                using (fileWriter)
                {
                    while (!(dataCollection.IsEmpty && dataCollection.IsCompleted))
                    {
                        if (stop)
                        {
                            return;
                        }
                        DataBlock block = null;
                        while (!((BlockingQueue)dataCollection).TryTake(out block))
                        {
                            while (!dataCollection.CanTake.WaitOne(QUEUE_WAIT_TRYADD_TIMEOUT))
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
            OnCompleted();
        }
    }
}
