using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DataCollection
{
    public class BlockingQueue : IBlockingCollection
    {
        private readonly ConcurrentQueue<DataBlock> dataQueue;
        public bool IsCompleted { get; set; }

        public bool IsEmpty => dataQueue.IsEmpty; 
        public int BoundedCapacity { get; }
        public int Count => dataQueue.Count;
        public AutoResetEvent CanTake { get; }
        public AutoResetEvent CanAdd { get; }
        public BlockingQueue(int boundedCapacity = 7000)
        {
            this.BoundedCapacity = boundedCapacity;
            this.dataQueue = new ConcurrentQueue<DataBlock>();
            this.CanTake = new AutoResetEvent(false);
            this.CanAdd = new AutoResetEvent(true);
            this.IsCompleted = false;
        }
       
        public bool TryAdd(DataBlock item) 
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

        public bool TryTake(out DataBlock item)
        {
            bool result = false;
            item = null;
            if (!IsEmpty)
            {                 
                result = this.dataQueue.TryDequeue(out item);
                CanAdd.Set();
            }                              
            return result;
        }

    }
}
