using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileDispatchers;

namespace Gzip
{
    public class GZipCompressor:GZipper
    {
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
        }
        bool CheckExtensions()
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
            using (fileDispatcherFrom = new SimpleFileDispatcher(pathFrom, 1024 * 1024))
            {
                using (fileDispatcherTo = new
                    CompressedFileDispatcher(pathTo, "compress"))
                {                   
                    var numOfBlocksByte = BitConverter.
                        GetBytes(fileDispatcherFrom.NumberOfBlocks);
                    ((CompressedFileDispatcher)fileDispatcherTo)
                        .FileStream.Write(numOfBlocksByte, 0, numOfBlocksByte.Length);//TODO доступ к стриму класса-плохо                          
                    blockOperation = CompressBlock;
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
