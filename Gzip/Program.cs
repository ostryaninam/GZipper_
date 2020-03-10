using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace GZipLibrary
{
    class Program
    {
        private static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();
        static int Main(string[] args)
        {
            //string pathFrom = "";
            //string pathTo = "";
            //GZipOperation operation = 0;
            //string[] instructions = args;
            //IProcessManager processManager = null;
            //if (instructions.Length == 3 &&
            //    (instructions[0] == "compress" || instructions[0] == "decompress"))
            //{
            //    if (instructions[0] == "compress")
            //        operation = GZipOperation.Compress;
            //    else
            //        operation = GZipOperation.Decompress;
            //    pathFrom = instructions[1];
            //    pathTo = instructions[2];
            //}
            //else
            //{
            //    logger.Error("Wrong input file format");
            //    return -1;
            //}
            //if (CheckExtensions(pathFrom, pathTo))
            //{
            //    try
            //    {
            //        processManager = new GZipProcessManager(pathFrom, pathTo, operation);
            //        processManager.StartProcess().WaitOne();
            //        if (processManager.Exception == null)
            //            return 0;
            //        else
            //            throw processManager.Exception;
            //    }
            //    catch(Exception ex)
            //    {
            //        logger.Error(ex.Message);
            //        return -1;
            //    }
            //}
            //else
            //{
            //    logger.Error("Wrong input/output file format");
            //    return -1;
            //}
            try
            {
                GZipProcessManager manager = new GZipProcessManager(@"C:\mults\Rihter_CLR-via-C.gz",
                    @"C:\mults\Rihter_CLR-via-C1.pdf",
                    GZipOperation.Decompress);
                manager.StartProcess().WaitOne();
            }
            catch(Exception e)
            {
                logger.Error(e);
            }
            return 0;
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
