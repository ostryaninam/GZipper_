using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DataCollection
{
    public class BlockingQueue
    {
        private Queue<DataBlock> dataQueue;
        ManualResetEvent blockAdded;
        ManualResetEvent blockTaken;
        AutoResetEvent isEmpty;
        private int boundedCapacity;
        private object lockObject;

        public int BoundedCapacity { get => boundedCapacity; }
        public int Count => dataQueue.Count;
 
        public BlockingQueue()
        {
            this.boundedCapacity = 5000;
            dataQueue = new Queue<DataBlock>();
            blockTaken = new ManualResetEvent(false);
            lockObject = new object();
            blockAdded = new ManualResetEvent(false);
            isEmpty = new AutoResetEvent(true);
        }
       
        public bool TryAdd(DataBlock block) 
        {
            bool result = false;
            while (!result)
            {
                if (dataQueue.Count <= boundedCapacity)
                    lock (lockObject)
                    {
                        dataQueue.Enqueue(block);
                        result = true;
                        blockAdded.Set();
                        blockAdded.Reset();
                    }
                else
                {
                    blockTaken.WaitOne();
                }
            }
            return true;            
        }


        public bool TryTake(out DataBlock item)
        {
            bool result = false;
            item = null;
            while (!result)
            {
                if (dataQueue.Count > 0)
                {
                    lock (lockObject)
                    {
                        item = dataQueue.Dequeue();
                        result = true;
                        blockTaken.Set();
                        blockTaken.Reset();
                    }
                }
                else
                {
                    blockAdded.WaitOne();
                }
            }
            return true;
        }

    }
}
