using System;
using System.Collections.Generic;
using System.Text;

namespace FileManagerLibrary.Abstractions
{
    interface IFileFactory
    {
        public IFileFactory(string path, int )
        IFileReader GetFileReader();
        IFileWriter GetFileWriter();
    }
}
