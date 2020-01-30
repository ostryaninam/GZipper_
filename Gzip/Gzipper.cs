using FileDispatchers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gzip
{
    public delegate byte[] Operation(byte[] inputBytes);
    public abstract class GZipper
    {       
        protected IFileDispatcher fileDispatcherFrom;
        protected IFileDispatcher fileDispatcherTo;        
        protected Operation blockOperation;

        protected ConcurrentDictionary<long, byte[]> blocks;
        protected int indexResidue = 0;       
        protected readonly int dictionaryMaxLength = Environment.ProcessorCount*10;

        protected object locker = new object();
        
        protected CountdownEvent cde;
        Thread writingThread;

        public Thread Thread { get => writingThread;  }

        protected void DoGzipWork() 
        {
            int numberOfThreads = Environment.ProcessorCount;
            blocks = new ConcurrentDictionary<long, byte[]>();                       
            cde = new CountdownEvent(numberOfThreads + 1);
            for (int i = 0; i < numberOfThreads; i++)
            {
                Thread current = new Thread(new ThreadStart(GzipThread));
                current.Start();
            }
            writingThread = new Thread(new ThreadStart(WritingThread));
            writingThread.Start();
            cde.Wait();
            Console.WriteLine("Успешно");
        }
        void GzipThread()
        {
            while (!fileDispatcherFrom.EndOfFile)
            {
                byte[] fileBlock = null;
                long indexOfBlock;
                lock (locker)
                {
                    indexOfBlock = fileDispatcherFrom.CurrentIndexOfBlock;
                    fileBlock = fileDispatcherFrom.GetBlock();
                }
                var outputBlock = blockOperation(fileBlock);
               
                while (indexOfBlock / dictionaryMaxLength != indexResidue)
                {
                    Thread.Sleep(10);
                }
                blocks.TryAdd(indexOfBlock, outputBlock);
                
            }           
            cde.Signal();
        }
        void WritingThread()
        {
            long writtenBlocks = 0;
            while (fileDispatcherFrom.NumberOfBlocks > writtenBlocks)//TODO может тут говно
            {
                for (long i = indexResidue * dictionaryMaxLength;
                    i < dictionaryMaxLength * (indexResidue + 1); i++)
                {
                    if (fileDispatcherFrom.NumberOfBlocks > writtenBlocks)
                    {
                        byte[] currentBlock;
                        while (!blocks.TryRemove(i, out currentBlock))
                        {
                            Thread.Sleep(10);
                        }
                        fileDispatcherTo.WriteBlock(currentBlock);
                        writtenBlocks++;

                    }
                    else
                        break;  
                }
                indexResidue++;
               
            }
                
            cde.Signal();
        }
        
    }
}
