using System;
using System.Collections.Generic;
using System.Text;

namespace FileManagerLibrary.Abstractions
{
    public interface IFileReader : IDisposable
    {
        bool EndOfFile { get; }
        long NumberOfBlocks { get; }
        long CurrentIndexOfBlock { get; }
        byte[] ReadBlock();
    }
}
