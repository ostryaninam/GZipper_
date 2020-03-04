using System;
using System.Collections.Generic;
using System.Text;
using FileManagerLibrary.Abstractions;

namespace FileManagerLibrary.Implementations
{
    public class CompressedFileFactory : IFileFactory
    {
        private readonly string path;
        public CompressedFileFactory(string path)
        {
            this.path = path;
        }
        public IFileReader GetFileReader()
        {
            return new CompressedFileReader(path);
        }

        public IFileWriter GetFileWriter()
        {
            return new CompressedFileWriter(path);
        }
    }
}
