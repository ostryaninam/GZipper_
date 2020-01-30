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

        int indexResidue = 0;       
        readonly int dictionaryMaxLength = Environment.ProcessorCount*10;
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
                }
                var outputBlock = GZipOperation(fileBlock);

                if (indexOfBlock / dictionaryMaxLength != indexResidue)
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
                for (long i = indexResidue * dictionaryMaxLength;   //TODO bad construction
                    i < dictionaryMaxLength * (indexResidue + 1); i++)
                {
                    if (fileFrom.NumberOfBlocks > writtenBlocks)
                    {
                        if (!blocks.TryRemove(i, out var currentBlock))
                            readyBlockEvent.WaitOne();

                        fileTo.WriteBlock(currentBlock);
                        writtenBlocks++;
                    }
                    else
                        break;  
                }
                indexResidue++;
                canWrite.Set();
            }                
            endSignal.Signal();
        }        
    }
}
