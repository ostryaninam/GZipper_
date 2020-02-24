using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;

namespace FixedThreadPool
{
    public class FixedThreadPool : IDisposable
    {
        private static FixedThreadPool instance;
        private int threadsCount;
        private static bool isWorking;
        private bool isStopping;
        private CountdownEvent stopSignal;
        private AutoResetEvent managerEvent;
        private bool isDisposed;
        private object stoplock = new object();
        
        private ConcurrentQueue<Action> actions;
        private Dictionary<int, AutoResetEvent> wakeEvents;
        private Thread[] threads;                               
        private Thread managerThread;

        public int Count { get => threadsCount; }
        public static bool IsWorking { get => isWorking; }
        private FixedThreadPool(BlockingCollection<Action> actionQueue)
        {
            actionQueue.
            threadsCount = Environment.ProcessorCount;
            wakeEvents = new Dictionary<int, AutoResetEvent>();
            actions = new ConcurrentQueue<MyTask>();
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
            managerEvent = new AutoResetEvent(false);
            managerThread = new Thread(ManagerThreadWork) { IsBackground = true };
            managerThread.Start();
            for(int i = 0; i < threadsCount; i++)
            {
                Thread thread = new Thread(ThreadWork) { IsBackground = true };
                wakeEvents.Add(thread.ManagedThreadId, new AutoResetEvent(false));
                threads[i] = thread;
                threads[i].Start();
            }
            isWorking = true;
        }
        public bool Execute(Action action)
        {
            lock (stoplock)
            {
                if (!isStopping)
                {
                    MyTask task = new MyTask(action);
                    AddAndStartNewTask(task);
                    return true;
                }
                else
                    return false;
            }
        }
        private void ThreadWork()
        {
            while (true)
            {
                wakeEvents[Thread.CurrentThread.ManagedThreadId].WaitOne();

                if (Remove(out var task))         
                {
                    lock (stoplock)
                    {
                        if (isStopping)
                            stopSignal.Signal();
                    }
                    task.Execute();
                }                
            }
        }
        private bool Remove(out MyTask task)
        {
            bool result=actions.TryDequeue(out task); 
            if (actions.Count > 0)  
                managerEvent.Set();
            return result;
        }
        private void AddAndStartNewTask(MyTask task)
        {
            actions.Enqueue(task);
            managerEvent.Set();
        }
        private void ManagerThreadWork()
        {
            while (true)
            {
                managerEvent.WaitOne();
                foreach (var t in wakeEvents.Values)    //free all tasks
                {
                    t.Set();
                }
            }
        }

        public void Stop()                          
        {
            lock (stoplock)
            {
                isStopping = true;
                stopSignal = new CountdownEvent(actions.Count);
            } 
            stopSignal.Wait();                              //wait for all tasks to end
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    managerThread.Abort();
                    managerEvent.Dispose();

                    foreach (var th in threads)
                    {
                        th.Abort();
                        wakeEvents[th.ManagedThreadId].Dispose();
                    }
                }
                isDisposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
