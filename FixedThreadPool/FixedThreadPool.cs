using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using DataCollection;

namespace FixedThreadPool
{
    public class FixedThreadPool : IDisposable
    {
        private static FixedThreadPool instance;
        private int threadsCount;
        private bool isWorking;
        private bool isStopping;
        private CountdownEvent stopSignal;
        private object stoplock = new object();
        
        private BlockingQueue<Action> actions;
        private Thread[] threads;                               

        public int Count { get => threadsCount; }
        public bool IsStopping { get => isStopping; }
        public bool IsWorking { get => isWorking; }

        private FixedThreadPool()
        {
            this.actions = new BlockingQueue<Action>();
            if (Environment.ProcessorCount > 2)
                this.threadsCount = Environment.ProcessorCount - 2;
            else
                threadsCount = 1;
            this.threads = new Thread[threadsCount];
            Start();
        }
        public static FixedThreadPool GetInstance()
        {
            if (instance == null)
                instance = new FixedThreadPool();
            return instance;
        }
        private void Start()
        {
            for(int i = 0; i < threadsCount; i++)
            {
                Thread thread = new Thread(ThreadWork) { IsBackground = true };
                threads[i] = thread;
                threads[i].Start();
            }
            isWorking = true;
        }
        public bool Execute(Action action)
        {
            bool result = false;
            lock (stoplock)
            {
                if (!isStopping)
                {
                    result = actions.TryAdd(action);                     
                }
            }
            return result;
        }
        private void ThreadWork()
        {
            var stop = false;
            while (!stop)
            {                
                if (actions.TryTake(out var action))         
                {
                    lock (stoplock)
                    {
                        if (isStopping)
                        {
                            stop = true;
                            stopSignal.Signal();                            
                        }
                    }
                    action();
                }                
            }
        }       
        public void Dispose()
        {
            if (IsWorking)
            {
                lock (stoplock)
                {
                    isStopping = true;
                    stopSignal = new CountdownEvent(actions.Count);
                }
                stopSignal.Wait();
            }
        }
    }
}
