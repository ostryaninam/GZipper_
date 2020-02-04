using System;
using System.Collections.Generic;
using System.Text;

namespace ExceptionsHandling
{
    public sealed class FileSizeException : Exception
    {
        public FileSizeException(string message)
            : base(message)
        { }

        public FileSizeException()
            :base("File size is less then 1 Mb")
        { }
    }
}
