using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DataCollection
{
    public class BlockingDataCollection : IProducerConsumerCollection<DataBlock>
    {
        private Queue<DataBlock> dataQueue;
        ManualResetEvent blockAdded;
        ManualResetEvent blockTaken;
        AutoResetEvent isEmpty;
        private int boundedCapacity;
        private object lockObject;

        public int BoundedCapacity { get => boundedCapacity; }
        public int Count => dataQueue.Count;
        public bool IsSynchronized => true;
        public object SyncRoot => lockObject;

        public BlockingDataCollection()
        {
            this.boundedCapacity = 5000;
            dataQueue = new Queue<DataBlock>();
            blockTaken = new ManualResetEvent(false);
            lockObject = new object();
            blockAdded = new ManualResetEvent(false);
            isEmpty = new AutoResetEvent(true);
        }
        public void CopyTo(DataBlock[] array, int index)
        {
            lock (lockObject)
            {
                dataQueue.CopyTo(array, index);
            }
        }

        public DataBlock[] ToArray()
        {
            DataBlock[] result = null;
            lock (lockObject)
            {
                result = dataQueue.ToArray();
            }
            return result;
        }

        public bool TryAdd(DataBlock block) 
        {
            if (dataQueue.Count<=boundedCapacity)
                lock (lockObject)
                {
                    dataQueue.Enqueue(block);   
                    blockAdded.Set();                             
                    blockAdded.Reset();
                }
            else
            {
                blockTaken.WaitOne();
                TryAdd(block);
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

        public IEnumerator<DataBlock> GetEnumerator()
        {
            Queue<DataBlock> queueCopy = null;
            lock (lockObject)
            {
                queueCopy = new Queue<DataBlock>(dataQueue);
            }
            return queueCopy.GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            lock (lockObject)
            {
                ((ICollection)dataQueue).CopyTo(array, index);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<DataBlock>)this).GetEnumerator();
        }
    }
}
