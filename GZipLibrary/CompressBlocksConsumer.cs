using DataCollection;
using FileManagerLibrary.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace GZipLibrary
{
    class CompressBlocksConsumer : BlocksConsumer
    {
        private bool EndCondition => dataCollection.IsEmpty && dataCollection.IsCompleted;
        public CompressBlocksConsumer(IFileWriter fileWriter, IBlockingCollection dataQueue) :
            base(fileWriter, dataQueue) { }
        public override void ThreadWork()
        {
            try
            {
                using (fileWriter)
                {
                    while (!EndCondition)
                    {
                        if (stop)
                        {
                            logger.Debug("Blocksconsumer completed working");
                            OnCompleted();
                            return;
                        }
                        DataBlock block = null;
                        while (!((BlockingQueue)dataCollection).TryTake(out block))
                        {
                            while (!dataCollection.CanTake.WaitOne(QUEUE_WAIT_TRYADD_TIMEOUT))
                                if (stop||EndCondition)
                                {
                                    logger.Debug("Blocksconsumer completed working");
                                    OnCompleted();
                                    return;
                                }
                        }
                        fileWriter.WriteBlock(block);
                        logger.Debug($"Blocksconsumer wrote block {block.Index}");
                    }
                }
                logger.Debug("Blocksconsumer completed working");
            }
            catch (Exception ex)
            {
                OnErrorOccured(ex);
            }
            OnCompleted();
        }
    }
}
