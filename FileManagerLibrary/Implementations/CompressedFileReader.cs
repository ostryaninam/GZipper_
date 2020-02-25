using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FileManagerLibrary.Abstractions;
using ExceptionsHandling;
using DataCollection;
using System.Collections;

namespace FileManagerLibrary.Implementations
{
    class CompressedFileReader : IFileReader, IEnumerator<DataBlock>
    {
        private int numberOfBlocks;
        private readonly FileStream fileStream;
        private DataBlock currentBlock;
        public CompressedFileReader(FileStream filestream)
        {
            this.fileStream = filestream;
        }
        public bool EndOfFile => fileStream.Position >= fileStream.Length;
        DataBlock IEnumerator<DataBlock>.Current => currentBlock;
        public object Current => currentBlock;
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

        public CompressedFileReader(string path)
        {
            try
            {
                this.fileStream = new FileStream(path, FileMode.Open);
                ReadNumberOfBlocks();
            }
            catch (IOException e)
            {
                ExceptionsHandler.Handle(this.GetType(),e);
            }
        }
        public int NumberOfBlocks { get => numberOfBlocks; }

        private void ReadNumberOfBlocks()
        {
            var byteNumberOfBlocks = new byte[4];
            this.fileStream.Read(byteNumberOfBlocks, 0, 4);
            numberOfBlocks = BitConverter.ToInt32(byteNumberOfBlocks, 0);
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
