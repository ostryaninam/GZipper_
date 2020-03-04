using System;
using System.Collections.Generic;
using System.Text;
using FileManagerLibrary.Abstractions;

namespace FileManagerLibrary.Implementations
{
    public class SimpleFileFactory : IFileFactory
    {
        private readonly string path;
        private readonly int blockSize;
        public SimpleFileFactory(string path,int blockSize)
        {
            this.path = path;
            this.blockSize = blockSize;
        }
        public IFileReader GetFileReader()
        {
            return new SimpleFileReader(path, blockSize);
        }

        public IFileWriter GetFileWriter()
        {
            return new SimpleFileWriter(path);
        }
    }
}
