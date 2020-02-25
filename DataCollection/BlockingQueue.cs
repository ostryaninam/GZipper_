using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DataCollection
{
    public class BlockingQueue<T>
    {
        private Queue<T> dataQueue;
        private AutoResetEvent itemAdded;
        private AutoResetEvent itemTaken;
        private AutoResetEvent isEmpty;
        private int boundedCapacity;
        private object lockObject;

        public bool IsCompleted { get; set; }

        public bool IsEmpty { get => dataQueue.Count==0; }
        public int BoundedCapacity { get => boundedCapacity; }
        public int Count => dataQueue.Count;
        public AutoResetEvent CanTake { get => itemAdded; }
        public AutoResetEvent CanAdd { get => itemTaken; }
        public BlockingQueue()
        {
            boundedCapacity = 5000;
            dataQueue = new Queue<T>();
            lockObject = new object();
            itemAdded = new AutoResetEvent(false);
            isEmpty = new AutoResetEvent(true);
            IsCompleted = false;
        }
       
        public bool TryAdd(T item) 
        {
            bool result = false;
            if (!IsCompleted && dataQueue.Count <= boundedCapacity)
                lock (lockObject)
                {
                    dataQueue.Enqueue(item);
                    result = true;
                    itemAdded.Set();
                }
            return result;            
        }

        public bool TryTake(out T item)
        {
            bool result = false;
            item = default(T);            
            if (!IsEmpty)
            {
                lock (lockObject)
                {
                    item = dataQueue.Dequeue();
                    result = true;
                    itemTaken.Set();
                }                              
            }
            return result;
        }

    }
}
