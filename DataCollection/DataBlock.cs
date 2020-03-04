using System;

namespace DataCollection
{
    public class DataBlock
    {
        public int Length => this.BlockBytes.Length; 
        public long Index { get; }
        public byte[] BlockBytes { get; }

        public DataBlock(long index, byte[] block)
        {
            this.Index = index;
            this.BlockBytes = block;
        }
    }
}
