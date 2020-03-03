﻿using System;
using NLog;
using NLog.Config;

namespace ExceptionsHandling
{
    public static class Logger
    {
        private static NLog.Logger logger;
        static Logger()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("NLog.config");
            logger = LogManager.GetCurrentClassLogger();
        }
        public static void Handle(Type source,Exception e)
        {
            logger.Error($"In {source}.cs: {e.Message}");
            Stop();
            GC.Collect();
            Environment.Exit(1);
        }
        public static void Log(string message)
        {
            logger.Info(message);
        }
        static void Stop()
        {
            //if (FixedThreadPool.FixedThreadPool.IsStopping)
            //    FixedThreadPool.FixedThreadPool.GetInstance().Stop();
        }


    }
}