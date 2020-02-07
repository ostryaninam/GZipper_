﻿using FileManagerLibrary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileManagerLibrary.Abstractions;
using FileManagerLibrary.Implementations;

namespace Gzip
{
    public class GZipCompressor : GZipper
    {
        FixedThreadPool.FixedThreadPool threadPool;
        string pathFrom;
        string pathTo;
        public GZipCompressor(string pathFrom,string pathTo)
        {
            this.pathFrom = pathFrom;
            this.pathTo = pathTo;
        }
        protected void DoGzipWork()
        {
            threadPool = FixedThreadPool.FixedThreadPool.GetInstance(); //TODO naming
            dataBlocks = new DataCollection.BlockingDataCollection(100);
            readyBlockEvent = new AutoResetEvent(false);
            canWrite = new ManualResetEvent(false);
            endSignal = new CountdownEvent(threadPool.Count);

            for (int i = 0; i < threadPool.Count - 1; i++)
                threadPool.Execute(() => GzipThreadWork());
            threadPool.Execute(() => WritingThreadWork());
            endSignal.Wait();
            Console.WriteLine("Успешно");
        }
      
        public void Compress()
        {
            using (IFileReader fileFrom = new SimpleFileFactory(pathFrom, 1024 * 1024).GetFileReader())
                using(IFileWriter fileTo = new CompressedFileFactory(pathTo).GetFileWriter())

            {                 
                    ((IFileWriter)fileTo).WriteLong(fileFrom.NumberOfBlocks);                          
                    DoGzipWork();                    
            }
        }

        protected override byte[] GZipOperation(byte[] inputBytes)
        {
            return CompressBlock(inputBytes);
        }
        byte[] CompressBlock(byte[] bytesToCompress)
        {
            byte[] resultBytes = null;

            try
            {
                using (MemoryStream streamTo = new MemoryStream())
                {
                    using (GZipStream gzipStream = new
                        GZipStream(streamTo, CompressionMode.Compress))
                    {
                        gzipStream.Write(bytesToCompress, 0, bytesToCompress.Length);
                        gzipStream.Flush();
                    }
                    resultBytes = streamTo.ToArray();
                }

                return resultBytes;
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}
