using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileManagerLibrary.Abstractions;
using DataCollection;

namespace Gzip
{
    public class FileDispatcher
    {
        private readonly IFileReader fileReader;
        private readonly IFileWriter fileWriter;
        FixedThreadPool.FixedThreadPool threadPool;
        public FileDispatcher(IFileReader filereader, IFileWriter filewriter)
        {
            fileReader = filereader;
            fileWriter = filewriter;
            threadPool = FixedThreadPool.FixedThreadPool.GetInstance();
        }

        public void ReadBlocks(BlockingQueue<DataBlock> producingQueue)
        {
            threadPool.Execute(() =>
            {
                using (fileReader)
                {
                    foreach (var block in fileReader)
                    {
                        if (!threadPool.IsStopping)
                        {
                            if (!producingQueue.TryAdd(block))
                                producingQueue.ItemTaken.WaitOne();
                        }
                        else
                            break;
                    }
                }
            });
        }

        public void WriteBlocks(BlockingQueue<DataBlock> consumingQueue)
        {
            threadPool.Execute(() =>
            {
                using (fileWriter)
                {
                    int writtenBlocks = 0;
                    while (fileReader.NumberOfBlocks > writtenBlocks)
                    {
                        if (!threadPool.IsStopping)
                        {
                            if (consumingQueue.TryTake(out var block))
                            {
                                fileWriter.WriteBlock(block);
                                writtenBlocks++;
                            }
                            else
                                consumingQueue.ItemAdded.WaitOne();
                        }
                        else
                            break;
                    }
                }
            });
        }
    }
}
