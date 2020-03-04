using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
          
            this.fileStream = File.OpenRead(path);
            this.blockSize = blockSize;
            this.NumberOfBlocks = (int)(this.fileStream.Length / blockSize);
                
            if (this.fileStream.Length % this.blockSize != 0)
                this.NumberOfBlocks++;
            
        }
        public DataBlock ReadBlock()
        {
            byte[] fileBlock = new byte[blockSize];
            var index = currentIndexOfBlock;
            if (this.fileStream.Length >= this.blockSize)
            {
                try
                {
                    this.fileStream.Read(fileBlock, 0, blockSize);
                    currentIndexOfBlock++;
                    return new DataBlock(index, fileBlock);
                }
                catch (EndOfStreamException)
                {
                    this.fileStream.Read(fileBlock, 0, (int)(this.fileStream.Length - this.fileStream.Position));
                    return new DataBlock(index, fileBlock);
                }
            }
            else
            {
                throw new Exception("File size is less than 1 Mb");
            }
        }

        public bool MoveNext()
        {
            this.currentBlock = ReadBlock();
            return !EndOfFile;
        }

        public void Reset()
        {
            this.fileStream.Position = 0;
        }
        
        public void Dispose()
        {
            this.fileStream.Close();
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
