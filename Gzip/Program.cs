﻿using GZipLibrary;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace MainApplication
{
    class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        static int Main(string[] args)
        {
            logger.Info("Hello!");
            string pathFrom = "";
            string pathTo = "";
            GZipOperation operation = 0;
            string[] instructions = args;
            IProcessManager processManager = null;
            if (instructions.Length == 3 &&
                (instructions[0] == "compress" || instructions[0] == "decompress"))
            {

                if (instructions[0] == "compress")
                    operation = GZipOperation.Compress;
                else
                    operation = GZipOperation.Decompress;
                pathFrom = instructions[1];
                pathTo = instructions[2];
            }
            else
            {
                logger.Error("Wrong input file format");
                return -1;
            }
            if (CheckExtensions(pathFrom, pathTo, operation))
            {
                try
                {
                    processManager = new GZipProcessManager(pathFrom, pathTo, operation);
                    logger.Info("Process started");
                    processManager.StartProcess().WaitOne();
                    if (processManager.Exception == null)
                    {
                        logger.Info("Successful");
                        return 0;
                    }
                    else
                        throw processManager.Exception;
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    return -1;
                }
            }
            else
            {
                logger.Error("Wrong input/output file format");
                return -1;
            }

        }
        static bool CheckExtensions(string pathFrom, string pathTo, GZipOperation operation)
        {
            FileInfo fileFrom = new FileInfo(pathFrom);
            FileInfo fileTo = new FileInfo(pathTo);
            if (operation == GZipOperation.Compress)
            {
                if (fileFrom.Extension != ".gz" && fileTo.Extension == ".gz")
                    return true;
                else
                    return false;
            }
            else
            {
                if (fileFrom.Extension == ".gz" && fileTo.Extension != ".gz")
                    return true;
                else
                    return false;
            }
        }
    }

}
