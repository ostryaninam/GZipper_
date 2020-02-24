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
    class SimpleFileReaderIEnumerator : IEnumerator<DataBlock>
    {
        private readonly FileStream fileStream;
        private readonly int blockSize;
        private long currentIndexOfBlock = 0;
        public SimpleFileReaderIEnumerator(FileStream filestream, int blocksize)
        {
            fileStream = filestream;
            blockSize = blocksize;
        }

        public bool EndOfFile => (fileStream.Position >= fileStream.Length);
        public DataBlock Current => ReadBlock();

        object IEnumerator.Current => ReadBlock();

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
            
        }

        public bool MoveNext()
        {
            return EndOfFile;
        }

        public void Reset()
        {
            fileStream.Position = 0;
        }
    }
    class SimpleFileReader : IFileReader
    {
        private readonly FileStream fileStream;
        private int numberOfBlocks = 0;
        private readonly int blockSize;
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

        public int NumberOfBlocks => numberOfBlocks;
        
        public void Dispose()
        {
            fileStream.Close();
        }

        public IEnumerator<DataBlock> GetEnumerator()
        {
            return new SimpleFileReaderIEnumerator(fileStream, blockSize);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
