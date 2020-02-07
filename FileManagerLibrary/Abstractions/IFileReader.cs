using System;
using System.Collections.Generic;
using System.Text;
using DataCollection;

namespace FileManagerLibrary.Abstractions
{
    public interface IFileReader : IDisposable
    {
        bool EndOfFile { get; }
        long NumberOfBlocks { get; }
        long CurrentIndexOfBlock { get; }
        DataBlock ReadBlock();
    }
}
