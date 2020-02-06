using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DataCollection
{
    class BlockingDataCollection : IProducerConsumerCollection<DataBlock>
    {
        private Queue<DataBlock> dataQueue;
        ManualResetEvent blockAdded;
        ManualResetEvent newDataSet;
        AutoResetEvent isEmpty;
        private int boundedCapacity;
        private int dataSetIndex = 0;
        private int dataSetCount = 0;
        private object lockObject;

        public int BoundedCapacity { get => boundedCapacity; set { boundedCapacity = value; } }
        public int Count => dataQueue.Count;
        public bool IsSynchronized => true;

        public object SyncRoot => lockObject;

        public BlockingDataCollection()
        {
            dataQueue = new Queue<DataBlock>();
            lockObject = new object();
            blockAdded = new ManualResetEvent(false);
            newDataSet = new ManualResetEvent(false);
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

        public bool TryAdd(DataBlock item) 
        {
            if (item.Index / boundedCapacity == dataSetIndex) //if index is in dataset bounds
                lock (lockObject)
                {
                    dataQueue.Enqueue(item);    //add item
                    blockAdded.Set();           //set the event of adding new block
                    dataSetCount++;             //how many blocks from current dataset added
                    if (dataSetCount == boundedCapacity) //if all blocks are already added
                    {
                        isEmpty.WaitOne();          //waits for all dataset blocks are got
                        NewDataSet();
                    }
                    blockAdded.Reset();
                }
            else
            {
                newDataSet.WaitOne();
                TryAdd(item);
            }
            return true;            
        }

        private void NewDataSet()
        {
            dataSetCount = 0;
            dataSetIndex++;
            newDataSet.Set();
            newDataSet.Reset();
        }

        public bool TryTake(out DataBlock item)
        {
            if (dataQueue.Count > 0)
            {
                lock (lockObject)
                    item = dataQueue.Dequeue();
            }
            else
            {
                isEmpty.Set();
                blockAdded.WaitOne();
                TryTake(out item);
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
