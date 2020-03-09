using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DataCollection
{
    public class BlockingDictionary : IBlockingCollection
    {
        private readonly ConcurrentDictionary<long,DataBlock> dataDictionary;
        public bool IsCompleted { get; set; }
        public bool IsEmpty => dataDictionary.IsEmpty;
        public int BoundedCapacity { get; }
        public int Count => dataDictionary.Count;
        public AutoResetEvent CanTake { get; }
        public AutoResetEvent CanAdd { get; }


        public BlockingDictionary(int boundedCapacity = 7000)
        {
            this.dataDictionary = new ConcurrentDictionary<long, DataBlock>();
            this.BoundedCapacity = boundedCapacity;
            this.CanAdd = new AutoResetEvent(true);
            this.CanTake = new AutoResetEvent(false);
        }

        public bool TryAdd(DataBlock block)
        {
            bool result = false;

            if (this.dataDictionary.Count <= BoundedCapacity && !IsCompleted)
            {
                result = this.dataDictionary.TryAdd(block.Index, block);
                CanTake.Set();
            }

            return result;
        }


        public bool TryTake(long key, out DataBlock item)
        {
            item = null;
            bool result = false;

            if (!IsEmpty)
            {
                result = this.dataDictionary.TryRemove(key, out item);
                CanAdd.Set();
            }
            
            return result;
        }
    }
}

