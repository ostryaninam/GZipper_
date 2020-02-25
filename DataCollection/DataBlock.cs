using System;

namespace DataCollection
{
    public class DataBlock
    {
        public int Length => this.GetBlockBytes.Length; 
        public long Index { get; }
        public byte[] GetBlockBytes { get; }

        public DataBlock(long index, byte[] block)
        {
            this.Index = index;
            this.GetBlockBytes = block;
        }
    }
}
