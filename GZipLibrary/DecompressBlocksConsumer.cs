using DataCollection;
using FileManagerLibrary.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace GZipLibrary
{
    class DecompressBlocksConsumer : BlocksConsumer
    {
        public DecompressBlocksConsumer(IFileWriter fileWriter, IBlockingCollection dataQueue) :
            base(fileWriter, dataQueue) { }
        public override void ThreadWork()
        {
            long indexOfBlock = 0;
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
                        while (!((BlockingDictionary)dataCollection).TryTake(indexOfBlock, out block))
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
