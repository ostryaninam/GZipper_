using System;
using System.Collections.Generic;
using System.Text;
using DataCollection;

namespace FileManagerLibrary.Abstractions
{
    public interface IFileWriter : IDisposable
    {
        void WriteBlock(DataBlock block);
        void WriteLong(long value);
    }
}
