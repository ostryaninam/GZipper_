using System;
using System.Collections.Generic;
using System.Text;

namespace FileManagerLibrary.Abstractions
{
    interface IFileFactory
    {
        IFileReader GetFileReader();
        IFileWriter GetFileWriter();
    }
}
