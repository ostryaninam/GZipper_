using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ExceptionsHandling;
using FileManagerLibrary.Abstractions;
using DataCollection;

namespace FileManagerLibrary.Implementations
{
    class SimpleFileWriter : IFileWriter
    {
        FileStream fileStream;

        public SimpleFileWriter(string path)
        {
            try
            {
                fileStream = File.Create(path);
            }
            catch(IOException e)
            {
                ExceptionsHandler.Handle(this.GetType(), e);
            }
        }

        public void WriteBlock(DataBlock block)
        {
            fileStream.Write(block.GetBlockBytes, 0, block.Length);
        }

        public void WriteLong(long value) { }

        public void Dispose()
        {
            fileStream.Close();
        }
    }
}
