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
using FileManagerLibrary.Abstractions;
using ExceptionsHandling;
using DataCollection;

namespace Gzip
{
    public abstract class GZipper
    {
        protected IFileReader fileFrom;
        protected IFileWriter fileTo;        
        protected CountdownEvent endSignal;
        protected AutoResetEvent readyBlockEvent;
        protected ManualResetEvent canWrite;
        protected BlockingDataCollection dataBlocks;

        int setIndex = 0;       
        readonly int blocksSet = Environment.ProcessorCount*10;
        object fileReadLocker = new object();

            
        //protected void GzipThreadWork()
        //{
        //    while (!fileFrom.EndOfFile)
        //    {
        //        byte[] fileBlock = null;
        //        long indexOfBlock = 0;
        //        byte[] result = null;    //TODO I don't need datablock index in compressor
        //        try
        //        {
        //            lock (fileReadLocker)
        //            {
        //                indexOfBlock = fileFrom.CurrentIndexOfBlock;
        //                fileBlock = fileFrom.ReadBlock();
        //            }
        //            result=GZipOperation(fileBlock);
        //        }
        //        catch(Exception e)
        //        {
        //            ExceptionsHandler.Handle(this.GetType(), e);
        //        }
        //        dataBlocks.TryAdd(result);                
        //    }           
        //    endSignal.Signal();
        //}
        //protected void WritingThreadWork()
        //{
        //    long writtenBlocks = 0;
        //    while (fileFrom.NumberOfBlocks > writtenBlocks)
        //    {
        //        dataBlocks.TryTake(out var item);
        //        fileTo.WriteBlock(item);
        //        writtenBlocks++;                                   
        //    }                
        //    endSignal.Signal();
        //}
        protected abstract byte[] GZipOperation(byte[] inputBytes);
        
    }
}
