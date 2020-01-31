using FileManagerLibrary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FixedThreadPool;

namespace Gzip
{
    public delegate byte[] GZipBlockOperation(byte[] inputBytes);
    public abstract class GZipper
    {
        protected IFileDispatcher fileFrom;
        protected IFileDispatcher fileTo;        
        protected GZipBlockOperation GZipOperation;
        protected CountdownEvent endSignal;
        protected ConcurrentDictionary<long, byte[]> blocks;
        protected AutoResetEvent readyBlockEvent;
        protected ManualResetEvent canWrite;

        int setIndex = 0;       
        readonly int blocksSet = Environment.ProcessorCount*10;
        object fileReadLocker = new object();

            
        protected void GzipThreadWork()
        {
            while (!fileFrom.EndOfFile)
            {
                byte[] fileBlock = null;
                long indexOfBlock;
                lock (fileReadLocker)
                {
                    indexOfBlock = fileFrom.CurrentIndexOfBlock;
                    fileBlock = fileFrom.GetBlock();
                    Console.WriteLine(fileBlock);
                }
                var outputBlock = GZipOperation(fileBlock);

                if (indexOfBlock / blocksSet != setIndex)
                    canWrite.WaitOne();

                if (blocks.TryAdd(indexOfBlock, outputBlock))
                    readyBlockEvent.Set();
                
            }           
            endSignal.Signal();
        }
        protected void WritingThreadWork()
        {
            long writtenBlocks = 0;
            while (fileFrom.NumberOfBlocks > writtenBlocks)
            {
                canWrite.Reset();
                for (long i = setIndex * blocksSet; i < blocksSet * (setIndex + 1); i++)
                {
                    if (fileFrom.NumberOfBlocks > writtenBlocks)
                    {
                        while (!blocks.TryGetValue(i, out var block))
                            readyBlockEvent.WaitOne();
                        if (blocks.TryRemove(i, out var currentBlock))
                                fileTo.WriteBlock(currentBlock);
                        writtenBlocks++;
                    }
                    else
                        break;  
                }
                //GC.Collect();
                setIndex++;
                canWrite.Set();
            }                
            endSignal.Signal();
        }        
    }
}
