using ExceptionsHandling;
using FileManagerLibrary.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileManagerLibrary.Implementations
{
    class CompressedFileWriter : IFileWriter
    {
        FileStream fileStream;
        Int64 numberOfBlocks;

        public CompressedFileWriter(string path)
        {
            try
            {
                fileStream = new FileStream(path, FileMode.Create);
            }
            catch (IOException e)
            {
                ExceptionsHandler.Handle(e);
            }
        }
        public bool EndOfFile => fileStream.Position >= fileStream.Length;
        public long NumberOfBlocks { get => numberOfBlocks; }
        public void WriteLong(long value)
        {
            var valueBytes = BitConverter.GetBytes(value);
            fileStream.Write(valueBytes, 0, valueBytes.Length);
        }
        public void WriteBlock(byte[] block)
        {
            var bytesCount = BitConverter.GetBytes(block.Length);
            List<byte> blocksToWrite = new List<byte>();
            blocksToWrite.AddRange(bytesCount);
            blocksToWrite.AddRange(block);
            fileStream.Write(blocksToWrite.ToArray(), 0, blocksToWrite.Count);
        }
        public void Dispose()
        {
            fileStream.Close();
        }
    }
}
