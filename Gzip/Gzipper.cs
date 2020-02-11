﻿using FileManagerLibrary;
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
        public abstract void DoGZipWork();

        protected abstract void GzipThreadWork();

        protected abstract void WritingThreadWork();
        
        protected abstract byte[] GZipOperation(byte[] inputBytes);
        
    }
}
