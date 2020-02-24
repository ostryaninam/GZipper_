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
    class CompressedFileIEnumerator : IEnumerator<DataBlock>
    {
        private readonly FileStream fileStream;

        public CompressedFileIEnumerator(FileStream filestream)
        {
            fileStream = filestream;
        }
        public bool EndOfFile => fileStream.Position >= fileStream.Length;
        public DataBlock ReadBlock()
        {
            var index = ReadIndex();
            var lengthOfBlock = ReadInt32();
            byte[] fileBlock = new byte[lengthOfBlock];
            fileStream.Read(fileBlock, 0, lengthOfBlock);
            return new DataBlock(index, fileBlock);
        }
        private long ReadIndex()
        {
            byte[] byteLengthOfBlock = new byte[8];
            fileStream.Read(byteLengthOfBlock, 0, 8);
            return BitConverter.ToInt64(byteLengthOfBlock, 0);
        }

        private int ReadInt32()
        {
            byte[] byteLengthOfBlock = new byte[4];
            fileStream.Read(byteLengthOfBlock, 0, 4);
            return BitConverter.ToInt32(byteLengthOfBlock, 0);
        }

        DataBlock IEnumerator<DataBlock>.Current => ReadBlock();

        public object Current => throw new NotImplementedException();

        public bool MoveNext()
        {
            return EndOfFile;
        }

        public void Reset()
        {
            fileStream.Position = 0;
        }

        public void Dispose()
        {

        }
    }
    class CompressedFileReader : IFileReader
    {
        private FileStream fileStream;
        private int numberOfBlocks;
        public CompressedFileReader(string path)
        {
            try
            {
                fileStream = new FileStream(path, FileMode.Open);
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
            fileStream.Read(byteNumberOfBlocks, 0, 4);
            numberOfBlocks = BitConverter.ToInt32(byteNumberOfBlocks, 0);
        }

        public void Dispose()
        {
            fileStream.Close();
        }

        public IEnumerator<DataBlock> GetEnumerator()
        {
            return new CompressedFileIEnumerator(fileStream);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerator<DataBlock>)GetEnumerator());
        }

    }
}
