using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ExceptionsHandling;
using FileManagerLibrary.Abstractions;
using DataCollection;
using System.Collections;

namespace FileManagerLibrary.Implementations
{
    class SimpleFileReader : IFileReader, IEnumerator<DataBlock>
    {
        private readonly FileStream fileStream;
        private readonly int blockSize;
        private long currentIndexOfBlock = 0;
        private DataBlock currentBlock;
        public bool EndOfFile => currentIndexOfBlock > NumberOfBlocks;
        public DataBlock Current => currentBlock;
        object IEnumerator.Current => currentBlock;
        public int NumberOfBlocks { get; }
        public SimpleFileReader(string path, int blockSize)
        {
            FileInfo fileInfo = new FileInfo(path);
            try
            {
                fileStream = File.OpenRead(path);
                this.blockSize = blockSize;
                NumberOfBlocks = (int)(fileStream.Length / blockSize);
                if (NumberOfBlocks == 0)
                {
                    ExceptionsHandler.Handle(this.GetType(), new FileSizeException());
                }
                if (fileStream.Length % blockSize != 0)
                    NumberOfBlocks++;
            }
            catch (IOException e)
            {
                ExceptionsHandler.Handle(this.GetType(), e);
            }
        }
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

        public bool MoveNext()
        {
            this.currentBlock = ReadBlock();
            return !EndOfFile;
        }

        public void Reset()
        {
            fileStream.Position = 0;
        }
        
        public void Dispose()
        {
            fileStream.Close();
        }

        public IEnumerator<DataBlock> GetEnumerator()
        {
            return (IEnumerator<DataBlock>)this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
