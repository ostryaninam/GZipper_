using System;
using System.Collections.Generic;
using System.Text;
using DataCollection;

namespace FileManagerLibrary.Abstractions
{
    public interface IFileReader : IDisposable, IEnumerable<DataBlock>
    {
        int NumberOfBlocks { get; }
    }
}
