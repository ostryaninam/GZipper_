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
        private readonly FileStream fileStream;
        public CompressedFileWriter(string path)
        {
            this.fileStream = new FileStream(path, FileMode.Create);
        }
        private void WriteIndex(long value)
        {
            var valueBytes = BitConverter.GetBytes(value);
            this.fileStream.Write(valueBytes, 0, valueBytes.Length);
        }
        public void WriteInt32(int value)
        {
            var valueBytes = BitConverter.GetBytes(value);
            this.fileStream.Write(valueBytes, 0, valueBytes.Length);
        }
        public void WriteBlock(DataBlock block)
        {
            WriteIndex(block.Index);
            WriteInt32(block.Length);
            this.fileStream.Write(block.BlockBytes, 0, block.Length);
        }

        public void Dispose()
        {
            this.fileStream.Close();
        }
    }
}
