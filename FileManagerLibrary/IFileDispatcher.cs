using System;
using System.Collections.Generic;
using System.Text;

namespace FileManagerLibrary
{
    public interface IFileDispatcher:IDisposable
    {
        void WriteBlock(byte[] block);
        byte[] GetBlock();
        long CurrentIndexOfBlock { get; }
        long NumberOfBlocks { get;  }
        bool EndOfFile { get; }
    }
}
