using DataCollection;
using FileManagerLibrary.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace GZipLibrary
{
    class DecompressBlocksConsumer : BlocksConsumer
    {
        private bool EndCondition => dataCollection.IsEmpty && dataCollection.IsCompleted;
        public DecompressBlocksConsumer(IFileWriter fileWriter, IBlockingCollection dataQueue) :
            base(fileWriter, dataQueue) { }
        public override void ThreadWork()
        {
            long indexOfBlock = 0;
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
                        while (!((BlockingDictionary)dataCollection).TryTake(indexOfBlock, out block))
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
                        indexOfBlock++;
                    }
                    logger.Debug($"Blocksconsumer ended working");
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
