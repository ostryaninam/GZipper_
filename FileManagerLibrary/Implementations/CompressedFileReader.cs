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
                var byteNumberOfBlocks = new byte[8];
                fileStream.Read(byteNumberOfBlocks, 0, 8);
                numberOfBlocks = BitConverter.ToInt64(byteNumberOfBlocks, 0);
            }
            catch (IOException e)
            {
                ExceptionsHandler.Handle(e);
            }
        }
        public bool EndOfFile => fileStream.Position >= fileStream.Length;
        public long CurrentIndexOfBlock { get => currentIndexOfBlock; } 
        public long NumberOfBlocks { get => numberOfBlocks; }
        public byte[] ReadBlock()
        {
            byte[] byteLengthOfBlock = new byte[4];
            fileStream.Read(byteLengthOfBlock, 0, 4);
            var lengthOfBlock = BitConverter.ToInt32(byteLengthOfBlock, 0);
            byte[] fileBlock = new byte[lengthOfBlock];
            fileStream.Read(fileBlock, 0, lengthOfBlock);
            currentIndexOfBlock++;
            return fileBlock;
        }
        public void Dispose()
        {
            fileStream.Close();
        }
    }
}
