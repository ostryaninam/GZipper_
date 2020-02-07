using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ExceptionsHandling;
using FileManagerLibrary.Abstractions;
using DataCollection;

namespace FileManagerLibrary.Implementations
{
    class SimpleFileReader : IFileReader
    {
        FileStream fileStream;
        long numberOfBlocks = 0;
        int blockSize;
        long currentIndexOfBlock = 0;
        public SimpleFileReader(string path, int blockSize)
        {
            FileInfo fileInfo = new FileInfo(path);
            try
            {
                fileStream = File.OpenRead(path);
                this.blockSize = blockSize;
                numberOfBlocks = fileStream.Length / blockSize;
                if (numberOfBlocks == 0)
                {
                    ExceptionsHandler.Handle(this.GetType(), new FileSizeException());
                }
                if (fileStream.Length % blockSize != 0)
                    numberOfBlocks++;
            }
            catch (IOException e)
            {
                ExceptionsHandler.Handle(this.GetType(),e);
            }
        }
        public long CurrentIndexOfBlock => currentIndexOfBlock;

        public long NumberOfBlocks => numberOfBlocks;

        public bool EndOfFile => (fileStream.Position >= fileStream.Length);

        public DataBlock ReadBlock()
        {
            byte[] fileBlock = new byte[blockSize];

            if (fileStream.Length >= blockSize)
            {
                try
                {
                    fileStream.Read(fileBlock, 0, blockSize);
                    var result = new DataBlock(currentIndexOfBlock, fileBlock);
                    currentIndexOfBlock++;
                    return result;
                }
                catch (EndOfStreamException)
                {
                    fileStream.Read(fileBlock, 0, (int)(fileStream.Length - fileStream.Position));
                    return new DataBlock(currentIndexOfBlock, fileBlock);
                }

            }
            else
            {
                throw new FileSizeException();
            }
        }

        public void Dispose()
        {
            fileStream.Close();
        }

    }
}
