using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Gzip
{
    class Program
    {
        static int Main(string[] args)
        {               
            string pathFrom = "";
            string pathTo = "";
            string operation = "";
            string[] instructions = args;
            GZipper gzipper = null;
            if (instructions.Length ==3 &&
                (instructions[0] == "compress" || instructions[0] == "decompress"))
            {
                operation = instructions[0];
                pathFrom = instructions[1];
                pathTo = instructions[2];                        
            }
            else
            {
                Console.WriteLine("Неверный формат ввода");
                return -1;
            }
            if (CheckExtensions(pathFrom, pathTo))
            {
                if (operation == "compress")
                {
                    gzipper = new GZipCompressor(pathFrom, pathTo);
                    ((GZipCompressor)gzipper).Compress();
                }
                if (operation == "decompress")
                {
                    gzipper = new GZipDecompressor(pathFrom, pathTo);
                    ((GZipDecompressor)gzipper).Decompress();
                }
                return 0;
            }
            else
            {
                Console.WriteLine("Неверный формат входного/выходного файла");
                return -1;
            }

        }
        static bool CheckExtensions(string pathFrom, string pathTo)                             
        {
            FileInfo fileFrom = new FileInfo(pathFrom);
            FileInfo fileTo = new FileInfo(pathTo);
            if (fileFrom.Extension != ".gz" && fileTo.Extension == ".gz")
                return true;
            else
                return false;
        }
    }

}
