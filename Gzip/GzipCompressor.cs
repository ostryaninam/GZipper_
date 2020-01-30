using FileManagerLibrary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gzip
{
    public class GZipCompressor:GZipper
    {
        FixedThreadPool.FixedThreadPool threadPool;
        string pathFrom;
        string pathTo;
        public GZipCompressor(string pathFrom,string pathTo)
        {
            this.pathFrom = pathFrom;
            this.pathTo = pathTo;
            if (!CheckExtensions())
            {
                Console.WriteLine("Ошибка: неверный формат файла");
                Environment.Exit(1);
            }
            DoGzipWork();
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
        bool CheckExtensions()                              //TODO to FileDispatcher
        {
            FileInfo fileFrom = new FileInfo(pathFrom);
            FileInfo fileTo = new FileInfo(pathTo);
            if (fileFrom.Extension != ".gz" && fileTo.Extension == ".gz")
                return true;
            else
                return false;
        }
        public void Compress()
        {
            using (fileFrom = new SimpleFileDispatcher(pathFrom, 1024 * 1024))
            {
                using (fileTo = new
                    CompressedFileDispatcher(pathTo, "compress"))
                {
                    //write number of blocks
                    ((CompressedFileDispatcher)fileTo).WriteLong(fileFrom.NumberOfBlocks);                          
                    GZipOperation = CompressBlock;
                    DoGzipWork();                    
                }
            }
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
                Console.WriteLine("Ошибка сжатия: проблемы с содержимым файла");
                Environment.Exit(1);
                return resultBytes;
            }

        }
    }
}
