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
        private readonly ConcurrentQueue<T> dataQueue;
        private readonly int boundedCapacity;

        public bool IsCompleted { get; set; }

        public bool IsEmpty => dataQueue.IsEmpty; 
        public int BoundedCapacity { get => boundedCapacity; }
        public int Count => dataQueue.Count;
        public AutoResetEvent CanTake { get; }
        public AutoResetEvent CanAdd { get; }
        public BlockingQueue(int boundedCapacity = 7000)
        {
            this.boundedCapacity = boundedCapacity;
            this.dataQueue = new ConcurrentQueue<T>();
            this.CanTake = new AutoResetEvent(false);
            this.CanAdd = new AutoResetEvent(true);
            this.IsCompleted = false;
        }
       
        public bool TryAdd(T item) 
        {
            bool result = false;
            if (!IsCompleted && this.dataQueue.Count <= BoundedCapacity)
            {
                this.dataQueue.Enqueue(item);
                result = true;
                CanTake.Set();
            }
            return result;            
        }

        public bool TryTake(out T item)
        {
            bool result = false;
            item = default(T);
            if (!IsEmpty)
            {                 
                result = this.dataQueue.TryDequeue(out item);
                CanAdd.Set();
            }                              
            return result;
        }

    }
}
