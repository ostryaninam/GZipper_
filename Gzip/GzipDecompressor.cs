using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using FileManagerLibrary;
using FileManagerLibrary.Abstractions;
using FileManagerLibrary.Implementations;
using FixedThreadPool;

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
        }
        protected void DoGzipWork()
        {
            threadPool = FixedThreadPool.FixedThreadPool.GetInstance();
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

        protected override byte[] GZipOperation(byte[] inputBytes)
        {
            return DecompressBlock(inputBytes);
        }
        public void Decompress()
        {
            using (fileFrom = new CompressedFileFactory(pathFrom).GetFileReader())
                using(fileTo = new SimpleFileFactory(pathTo, 1024 * 1024).GetFileWriter())
            {
                    DoGzipWork();
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
                throw;
            }
        }


    }
}
