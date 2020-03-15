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
                var completeTakingBlocks = new Func<bool>(() => dataCollection.IsEmpty && dataCollection.IsCompleted);
                using (fileWriter)
                {
                    while (!(completeTakingBlocks()))
                    {
                        if (stop)
                        {
                            return;
                        }
                        DataBlock block = null;
                        while (!((BlockingQueue)dataCollection).TryTake(out block))
                        {
                            while (!dataCollection.CanTake.WaitOne(QUEUE_WAIT_TRYADD_TIMEOUT))
                                if (stop||completeTakingBlocks())
                                {
                                    return;
                                }
                        }
                        fileWriter.WriteBlock(block);
                        logger.Debug($"Blocksconsumer wrote block {block.Index}");
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
