using DataCollection;
using ExceptionsHandling;
using FileManagerLibrary.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace FileManagerLibrary.Implementations
{
    class CompressedFileWriter : IFileWriter
    {
        FileStream fileStream;
        int dataSetLength;
        public CompressedFileWriter(string path, int dataSetLength)
        {
            this.dataSetLength = dataSetLength;
            try
            {
                fileStream = new FileStream(path, FileMode.Create);
            }
            catch (IOException e)
            {
                ExceptionsHandler.Handle(this.GetType(),e);
            }
        }
        public void WriteLong(long value)
        {
            var valueBytes = BitConverter.GetBytes(value);
            fileStream.Write(valueBytes, 0, valueBytes.Length);
        }
        public void WriteBlock(DataBlock block)
        {
            var lengthInBytes = BitConverter.GetBytes(block.Length);
            List<byte> blocksToWrite = new List<byte>();
            blocksToWrite.AddRange(lengthInBytes);
            blocksToWrite.AddRange(block.GetBlockBytes);
            fileStream.Write(blocksToWrite.ToArray(), 0, blocksToWrite.Count);
        }

        private void MakePlaceForTable()
        {
            byte[] emptyTable = new byte[dataSetLength*Int32.];
            for (int i = 0; i < dataSetLength; i++)
                emptyTable[i] = Byte.Parse(" ");
            fileStream.Write(emptyTable,0,emptyTable.);
        }
        public void Dispose()
        {
            fileStream.Close();
        }
    }
}
