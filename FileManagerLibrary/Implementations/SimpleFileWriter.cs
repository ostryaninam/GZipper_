using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ExceptionsHandling;
using FileManagerLibrary.Abstractions;

namespace FileManagerLibrary.Implementations
{
    class SimpleFileWriter : IFileWriter
    {
        FileStream fileStream;
        long numberOfBlocks = 0;

        public SimpleFileWriter(string path)
        {
            try
            {
                fileStream = File.Create(path);
            }
            catch(IOException e)
            {
                ExceptionsHandler.Handle(e);
            }
        }
        public long NumberOfBlocks => numberOfBlocks;

        public bool EndOfFile => (fileStream.Position >= fileStream.Length);

        public void WriteBlock(byte[] block)
        {
            fileStream.Write(block, 0, block.Length);
        }

        public void WriteLong(long value) { }

        public void Dispose()
        {
            fileStream.Close();
        }
    }
}
