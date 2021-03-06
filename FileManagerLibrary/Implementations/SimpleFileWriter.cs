﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FileManagerLibrary.Abstractions;
using DataCollection;

namespace FileManagerLibrary.Implementations
{
    class SimpleFileWriter : IFileWriter
    {
        private readonly FileStream fileStream;

        public SimpleFileWriter(string path)
        {
            this.fileStream = File.Create(path);
        }

        public void WriteBlock(DataBlock block)
        { 
            this.fileStream.Write(block.BlockBytes, 0, block.Length);
        }
        public void WriteInt32(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            this.fileStream.Write(BitConverter.GetBytes(value), 0, bytes.Length);
        }

        public void Dispose()
        {
            this.fileStream.Close();
        }
    }
}
