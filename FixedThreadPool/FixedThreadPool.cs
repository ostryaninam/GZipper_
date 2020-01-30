using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;

namespace FixedThreadPool
{
    public class FixedThreadPool:IDisposable
    {
        private int threadsCount;

        private bool isStopping;
        private CountdownEvent stopSignal;
        private AutoResetEvent managerEvent;
        private bool isDisposed;
        private object stoplock = new object();
        
        private List<Task> tasks;
        private Dictionary<int, AutoResetEvent> wakeEvents;
        private Thread[] threads;                               
        private Thread managerThread;

        public int Count { get => threadsCount; }

        public FixedThreadPool()
        {
            threadsCount = Environment.ProcessorCount;
            tasks = new List<Task>();
            threads = new Thread[threadsCount];
            Start();
        }
        private void Start()
        {
            managerEvent = new AutoResetEvent(false);
            managerThread = new Thread(ManagerThreadWork) { IsBackground = true };
            for(int i = 0; i < threadsCount; i++)
            {
                Thread thread = new Thread(ThreadWork) { IsBackground = true };
                wakeEvents.Add(thread.ManagedThreadId, new AutoResetEvent(false));
                threads[i] = thread;
                threads[i].Start();
            }
        }
        public bool Execute(Action action)
        {
            lock (stoplock)
            {
                if (!isStopping)
                {
                    Task task = new Task(action);
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
                var task = SelectTask();
                if (task!=null)
                {
                    task.Execute();
                    lock (stoplock)       
                    {
                        Remove(task);
                        if (isStopping)
                            stopSignal.Signal();
                    }
                }                
            }
        }
        private Task SelectTask()
        {
            Task task = null;
            lock (tasks)
            {
                task = tasks.FirstOrDefault(t => !t.IsRunning);
            }
            return task;
        }
        private void Remove(Task task)
        {
            lock (tasks)
            {
                tasks.Remove(task);
            }

            if (tasks.Count > 0 && tasks.Where(t => !t.IsRunning).Count() > 0)  //if there's a free task
                managerEvent.Set();
        }
        private void AddAndStartNewTask(Task task)
        {
            lock (tasks)
            {
                tasks.Add(task);
            }
            managerEvent.Set();
        }
        private void ManagerThreadWork()
        {
            while (true)
            {
                managerEvent.WaitOne();
                foreach (var t in wakeEvents.Values)    //trying to find a free task
                {
                    if (!t.WaitOne())
                    {
                        t.Set();
                        break;
                    }
                }
            }
        }

        public void Stop()                          
        {
            lock (stoplock)
            {
                isStopping = true;
                stopSignal = new CountdownEvent(tasks.Count);
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
