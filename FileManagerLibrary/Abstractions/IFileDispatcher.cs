using System;
using System.Collections.Generic;
using System.Text;

namespace FileManagerLibrary
{
    public interface IFileDispatcher:IDisposable
    {
        long NumberOfBlocks { get;  }
        bool EndOfFile { get; }
    }
}
