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

        long tablePosition=0;     //position in table in the beginning of dataset
        long blocksPosition;    //position in current dataset
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
            MakePlaceForTable();
        }
        public void WriteLong(long value)
        {
            var valueBytes = BitConverter.GetBytes(value);
            fileStream.Write(valueBytes, 0, valueBytes.Length);
        }
        public void WriteInt32(int value)
        {
            var valueBytes = BitConverter.GetBytes(value);
            fileStream.Write(valueBytes, 0, valueBytes.Length);
        }
        public void WriteBlock(DataBlock block)
        {
            WriteInTable(block.Index, block.Length);
            fileStream.Write(block.GetBlockBytes, 0, block.Length);
            blocksPosition = fileStream.Position;
        }
        private void WriteInTable(long index, int length)
        {
            fileStream.Position = tablePosition;
            WriteLong(index);
            WriteInt32(length);
            tablePosition = fileStream.Position;
            fileStream.Position = blocksPosition;
        }
        private void MakePlaceForTable()
        {
            byte[] emptyTable = new byte[dataSetLength*(1+1)]; //4 bytes for length and one for index
            for (int i = 0; i < dataSetLength; i++)
                emptyTable[i] = Byte.Parse(" ");
            fileStream.Write(emptyTable,0,emptyTable.Length);
            blocksPosition = fileStream.Position;
        }
        public void Dispose()
        {
            fileStream.Close();
        }
    }
}
