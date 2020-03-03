using DataCollection;
using FileManagerLibrary.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace FileManagerLibrary.Implementations
{
    class CompressedFileWriter : IFileWriter
    {
        FileStream fileStream;
        public CompressedFileWriter(string path)
        {
            fileStream = new FileStream(path, FileMode.Create);
        }
        private void WriteIndex(long value)
        {
            var valueBytes = BitConverter.GetBytes(value);
            fileStream.Write(valueBytes, 0, valueBytes.Length);
        }
        public void WriteInt32(int value)
        {
            var valueBytes = BitConverter.GetBytes(value);
            fileStream.Write(valueBytes, 0, valueBytes.Length);
        }
        public void WriteBlock(DataBlock block)
        {
            WriteIndex(block.Index);
            WriteInt32(block.Length);
            fileStream.Write(block.GetBlockBytes, 0, block.Length);
        }

        public void Dispose()
        {
            fileStream.Close();
        }
    }
}
