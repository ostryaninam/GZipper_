using System;

namespace DataCollection
{
    public class DataBlock
    {
        private int index;
        private byte[] block;
        public int Length { get => block.Length; }
        public int Index { get => index; }
        public byte[] Block { get => block; }

        public DataBlock(int index, byte[] block)
        {
            this.index = index;
            this.block = block;
        }
    }
}
