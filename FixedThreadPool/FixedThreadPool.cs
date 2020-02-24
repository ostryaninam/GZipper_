﻿using System;
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
        private Dictionary<int, AutoResetEvent> wakeEvents;
        private Thread[] threads;                               

        public int Count { get => threadsCount; }
        public bool IsStopping { get => isStopping; }
        public bool IsWorking { get => isWorking; }

        private FixedThreadPool()
        {
            actions = new BlockingQueue<Action>();
            threadsCount = Environment.ProcessorCount;
            wakeEvents = new Dictionary<int, AutoResetEvent>();
            threads = new Thread[threadsCount];
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
            var sequel = true;
            while (sequel)
            {                
                if (actions.TryTake(out var action))         
                {
                    lock (stoplock)
                    {
                        if (isStopping)
                        {
                            sequel = false;
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
