using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DataCollection
{
    public class BlockingDictionary 
    {
        private Dictionary<long, byte[]> dataDictionary;  //TODO do abstract class with T
        ManualResetEvent blockAdded;
        ManualResetEvent blockTaken;
        AutoResetEvent isEmpty;
        private int boundedCapacity;
        private object lockObject;

        public int BoundedCapacity { get => boundedCapacity; }
        public int Count => dataDictionary.Count;
        public bool IsSynchronized => true;
        public object SyncRoot => lockObject;

        public BlockingDictionary()
        {
            this.boundedCapacity = 5000;
            dataDictionary = new Dictionary<long, byte[]>();
            blockTaken = new ManualResetEvent(false);
            lockObject = new object();
            blockAdded = new ManualResetEvent(false);
            isEmpty = new AutoResetEvent(true);
        }

        public bool TryAdd(long index, byte[] block)
        {
            bool added = false;
            while (!added)
            {
                if (dataDictionary.Count <= boundedCapacity)
                    lock (lockObject)
                    {
                        dataDictionary.Add(index, block);
                        added = true;
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


        public bool TryTake(long key, out byte[] item)
        {
            item = null;
            bool result = false;
            while (!result)
            {
                lock (lockObject)
                {
                    result = dataDictionary.TryGetValue(key, out item);
                }
                if (result)
                {
                    dataDictionary.Remove(key);
                    blockTaken.Set();
                    blockTaken.Reset();
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

