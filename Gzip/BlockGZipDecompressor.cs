using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using FileManagerLibrary;
using FileManagerLibrary.Abstractions;
using FileManagerLibrary.Implementations;
using FixedThreadPool;
using DataCollection;

namespace Gzip
{
    public class BlockGZipDecompressor : IBlockGZipper
    {
        public event ErrorHandler ErrorOccured;

        public byte[] Execute(byte[] block)
        {
            byte[] resultBytes = null;

            using (MemoryStream streamFrom = new MemoryStream(block))
            {
                using (MemoryStream streamTo = new MemoryStream())
                {
                    using (GZipStream gzipStream = new
                        GZipStream(streamFrom, CompressionMode.Decompress))
                    {
                        gzipStream.CopyTo(streamTo);
                        gzipStream.Flush();
                    }
                    resultBytes = streamTo.ToArray();
                    streamTo.Flush();
                }
            }
            return resultBytes;
        }

    }
}
