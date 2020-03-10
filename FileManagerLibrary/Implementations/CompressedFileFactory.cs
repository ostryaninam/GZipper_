using System;
using System.Collections.Generic;
using System.Text;
using FileManagerLibrary.Abstractions;

namespace FileManagerLibrary.Implementations
{
    public class CompressedFileFactory : IFileFactory
    {
        private readonly string path;
        private readonly int numberOfBlocks;
        public CompressedFileFactory(string path)
        {
            this.path = path;
        }
        public CompressedFileFactory(string path, int numberOfBlocks) : this(path)
        {
            this.numberOfBlocks = numberOfBlocks;
        }
        public IFileReader GetFileReader()
        {
            return new CompressedFileReader(path);
        }

        public IFileWriter GetFileWriter()
        {
            return new CompressedFileWriter(path, numberOfBlocks);
        }
    }
}
