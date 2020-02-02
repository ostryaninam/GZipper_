﻿using System;
using System.Collections.Generic;
using System.Text;

namespace FileManagerLibrary.Abstractions
{
    public interface IFileReader : IFileDispatcher
    {
        long CurrentIndexOfBlock { get; }
        byte[] ReadBlock();
    }
}
