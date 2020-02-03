using System;
using NLog;
using NLog.Config;

namespace ExceptionsHandling
{
    public static class ExceptionsHandler
    {
        private static Logger logger;
        static ExceptionsHandler()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("NLog.config");
            logger = LogManager.GetCurrentClassLogger();
        }
        public static void Handle(Exception e)
        {
            logger.Info($"Exception in {e.StackTrace}: {e.Message}");
            Stop();
            GC.Collect();
            Environment.Exit(1);
        }
        static void Stop()
        {
            if (FixedThreadPool.FixedThreadPool.IsWorking)
                FixedThreadPool.FixedThreadPool.GetInstance().Stop();
        }
    }
}
