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
        int numberOfBlocks = 0;
        int blockSize;
        long currentIndexOfBlock = 0;
        public SimpleFileReader(string path, int blockSize)
        {
            FileInfo fileInfo = new FileInfo(path);
            try
            {
                fileStream = File.OpenRead(path);
                this.blockSize = blockSize;
                numberOfBlocks = (int)(fileStream.Length / blockSize);
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

        public int NumberOfBlocks => numberOfBlocks;

        public bool EndOfFile => (fileStream.Position >= fileStream.Length);

        public DataBlock ReadBlock()
        {
            byte[] fileBlock = new byte[blockSize];
            var index = currentIndexOfBlock;
            if (fileStream.Length >= blockSize)
            {
                try
                {
                    fileStream.Read(fileBlock, 0, blockSize);
                    currentIndexOfBlock++;
                    return new DataBlock(index, fileBlock);
                }
                catch (EndOfStreamException)
                {
                    fileStream.Read(fileBlock, 0, (int)(fileStream.Length - fileStream.Position));
                    return new DataBlock(index, fileBlock);
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
