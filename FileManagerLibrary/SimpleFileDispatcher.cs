using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace FileManagerLibrary
{
    public class SimpleFileDispatcher:IFileDispatcher
    {
        long numberOfBlocks = 0;
        int blockSize;
        FileStream fileStream;
        long currentIndexOfBlock = 0;
        public SimpleFileDispatcher(string path,int blockSize)
        {
            FileInfo fileInfo = new FileInfo(path);
            try
            {
                fileStream = File.OpenRead(path);                             
                this.blockSize = blockSize;
                numberOfBlocks = fileStream.Length / blockSize;
                if (numberOfBlocks == 0)
                {
                    Console.WriteLine("Ошибка: размер файла меньше 1 Мб");
                }
                if (fileStream.Length % blockSize != 0)
                    numberOfBlocks++;                               
            }
            catch(Exception)
            {
                Console.WriteLine("Ошибка: неверный путь к файлу");
                Environment.Exit(1);
            }
        }
        public SimpleFileDispatcher(string path)
        {
            try
            {
                fileStream = File.Create(path);
            }
            catch 
            {
                Console.WriteLine("Ошибка: неверный путь к файлу");
                Environment.Exit(1);
            }
        }
     
        public long NumberOfBlocks { get => numberOfBlocks; }
        public int BlockSize { get => blockSize; }
        public long CurrentIndexOfBlock { get => currentIndexOfBlock; }
        public bool EndOfFile {
            get
            {
                if (FileStream.Position >= FileStream.Length)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public FileStream FileStream { get => fileStream;  }

  
        public void Dispose()
        {
            fileStream.Close();
        }

        public byte[] GetBlock()
        {
            byte[] fileBlock = new byte[BlockSize];

            if (FileStream.Length >= BlockSize)
            {
                try
                {
                    FileStream.Read(fileBlock, 0, BlockSize);
                    currentIndexOfBlock++;
                    return fileBlock;
                }
                catch (EndOfStreamException)
                {
                    FileStream.Read(fileBlock, 0, (int)(FileStream.Length - FileStream.Position));
                    return fileBlock;
                }
                
            }
            else
            {
                FileStream.Read(fileBlock, 0, (int)FileStream.Length);
                return fileBlock;
            }
        }
        public void WriteBlock(byte[] block)
        {
            fileStream.Write(block, 0, block.Length);
        }
    }
}

