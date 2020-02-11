using System;

namespace DataCollection
{
    public class DataBlock
    {
        private long index;
        private byte[] block;
        public int Length { get => block.Length; }
        public long Index { get => index; }
        public byte[] GetBlockBytes { get => block; }

        public DataBlock(long index, byte[] block)
        {
            this.index = index;
            this.block = block;
        }
    }
}
