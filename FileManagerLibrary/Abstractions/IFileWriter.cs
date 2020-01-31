using System;
using System.Collections.Generic;
using System.Text;

namespace FileManagerLibrary.Abstractions
{
    public interface IFileWriter : IFileDispatcher
    {
        void WriteBlock(byte[] block);
        void WriteLong(long value);
    }
}
