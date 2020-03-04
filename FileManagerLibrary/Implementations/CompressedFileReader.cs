using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FileManagerLibrary.Abstractions;
using DataCollection;
using System.Collections;

namespace FileManagerLibrary.Implementations
{
    class CompressedFileReader : IFileReader, IEnumerator<DataBlock>
    {
        private readonly FileStream fileStream;
        private DataBlock currentBlock;
        public CompressedFileReader(FileStream filestream)
        {
            this.fileStream = filestream;
        }
        public bool EndOfFile => fileStream.Position >= fileStream.Length;
        public int NumberOfBlocks { get; }
        DataBlock IEnumerator<DataBlock>.Current => currentBlock;
        public object Current => currentBlock;

        public CompressedFileReader(string path)
        {
            this.fileStream = File.OpenRead(path);
            this.NumberOfBlocks = ReadNumberOfBlocks();
        }
        public DataBlock ReadBlock()
        {
            var index = ReadIndex();
            var lengthOfBlock = ReadInt32();
            byte[] fileBlock = new byte[lengthOfBlock];
            this.fileStream.Read(fileBlock, 0, lengthOfBlock);
            return new DataBlock(index, fileBlock);
        }
        private long ReadIndex()
        {
            byte[] byteLengthOfBlock = new byte[8];
            this.fileStream.Read(byteLengthOfBlock, 0, 8);
            return BitConverter.ToInt64(byteLengthOfBlock, 0);
        }

        private int ReadInt32()
        {
            byte[] byteLengthOfBlock = new byte[4];
            this.fileStream.Read(byteLengthOfBlock, 0, 4);
            return BitConverter.ToInt32(byteLengthOfBlock, 0);
        }

        public bool MoveNext()
        {        
            this.currentBlock = ReadBlock();
            return !EndOfFile;
        }

        public void Reset()
        {
            this.fileStream.Position = 0;
        }

        private int ReadNumberOfBlocks()
        {
            var byteNumberOfBlocks = new byte[4];
            this.fileStream.Read(byteNumberOfBlocks, 0, 4);
            return BitConverter.ToInt32(byteNumberOfBlocks, 0);
        }

        public void Dispose()
        {
            this.fileStream.Close();
        }

        public IEnumerator<DataBlock> GetEnumerator()
        {
            return (IEnumerator<DataBlock>)this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}
