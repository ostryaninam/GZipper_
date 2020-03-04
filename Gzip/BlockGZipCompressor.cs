using FileManagerLibrary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileManagerLibrary.Abstractions;
using FileManagerLibrary.Implementations;
using DataCollection;
using System.Diagnostics;
using NLog;

namespace Gzip
{
    public class BlockGZipCompressor : IBlockGZipper
    {
        public event ErrorHandler ErrorOccured;
        
        public byte[] Execute(byte[] block)
        {
            byte[] resultBytes = null;

            using (MemoryStream streamTo = new MemoryStream())
            {
                using (GZipStream gzipStream = new
                    GZipStream(streamTo, CompressionMode.Compress))
                {
                    gzipStream.Write(block, 0, block.Length);
                    gzipStream.Flush();
                }
                resultBytes = streamTo.ToArray();
            }

            return resultBytes;
        }     

    }
}
