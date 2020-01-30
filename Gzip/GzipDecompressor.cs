﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using FileManagerLibrary;

namespace Gzip
{
    public class GZipDecompressor: GZipper
    {
        string pathFrom;
        string pathTo;
        FixedThreadPool.FixedThreadPool threadPool;
        public GZipDecompressor(string pathFrom,string pathTo)
        {
            this.pathFrom = pathFrom;
            this.pathTo = pathTo;
            if (!CheckExtensions())
            {
                Console.WriteLine("Ошибка: неверный формат файла");
                Environment.Exit(1);
            }
        }
        protected void DoGzipWork()
        {
            threadPool = new FixedThreadPool.FixedThreadPool();
            blocks = new ConcurrentDictionary<long, byte[]>();
            readyBlockEvent = new AutoResetEvent(false);
            canWrite = new ManualResetEvent(false);
            endSignal = new CountdownEvent(threadPool.Count);

            for (int i = 0; i < threadPool.Count - 1; i++)
                threadPool.Execute(() => GzipThreadWork());
            threadPool.Execute(() => WritingThreadWork());
            endSignal.Wait();
            Console.WriteLine("Успешно");
        }
        bool CheckExtensions()
        {
            FileInfo fileFrom = new FileInfo(pathFrom);
            FileInfo fileTo = new FileInfo(pathTo);
            if (fileFrom.Extension == ".gz" && fileTo.Extension != ".gz")
                return true;
            else
                return false;
        }
        public void Decompress()
        {
            using (fileFrom = new
                   CompressedFileDispatcher(pathFrom,"decompress"))
            {
                using (fileTo = new SimpleFileDispatcher(pathTo))
                {                    
                    GZipOperation = DecompressBlock;
                    DoGzipWork();                   
                }
            }            
        }
        byte[] DecompressBlock(byte[] bytesToDecompress)
        {
            byte[] resultBytes = null;
            try
            {
                using (MemoryStream streamFrom = new MemoryStream(bytesToDecompress))
                {
                    using (MemoryStream streamTo = new MemoryStream())
                    {
                        using (GZipStream gzipStream = new
                            GZipStream(streamFrom, CompressionMode.Decompress))
                        {
                            gzipStream.CopyTo(streamTo);
                            gzipStream.Flush();
                        }
                        resultBytes = streamTo.ToArray();
                        streamTo.Flush();
                    }
                }

                return resultBytes;
            }
            catch (Exception)
            {
                Console.WriteLine("Ошибка распаковки: проблемы с содержимым файла");
                Environment.Exit(1);
                return resultBytes;
            }
        }


    }
}
