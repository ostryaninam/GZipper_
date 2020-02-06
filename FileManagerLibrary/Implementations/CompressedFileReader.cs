using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FileManagerLibrary.Abstractions;
using ExceptionsHandling;

namespace FileManagerLibrary.Implementations
{
    class CompressedFileReader : IFileReader
    {
        FileStream fileStream;
        long currentIndexOfBlock = 0;
        Int64 numberOfBlocks;
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
        public bool EndOfFile => fileStream.Position >= fileStream.Length;
        public long CurrentIndexOfBlock { get => currentIndexOfBlock; } 
        public long NumberOfBlocks { get => numberOfBlocks; }
        public byte[] ReadBlock()
        {
            var lengthOfBlock = ReadInt32();
            byte[] fileBlock = new byte[lengthOfBlock];
            fileStream.Read(fileBlock, 0, lengthOfBlock);
            currentIndexOfBlock++;
            return fileBlock;
        }
        private void ReadNumberOfBlocks()
        {
            var byteNumberOfBlocks = new byte[8];
            fileStream.Read(byteNumberOfBlocks, 0, 8);
            numberOfBlocks = BitConverter.ToInt64(byteNumberOfBlocks, 0);
        }

        private int ReadInt32()
        {
            byte[] byteLengthOfBlock = new byte[4];
            fileStream.Read(byteLengthOfBlock, 0, 4);
            return BitConverter.ToInt32(byteLengthOfBlock, 0);
        }
        public void Dispose()
        {
            fileStream.Close();
        }
    }
}
