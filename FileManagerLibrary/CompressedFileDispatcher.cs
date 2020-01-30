using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileDispatchers
{
    public class CompressedFileDispatcher:IDisposable,IFileDispatcher
    {
        FileStream fileStream;
        long currentIndexOfBlock = 0;
        Int64 numberOfBlocks;
        public CompressedFileDispatcher(string path,string operation)
        {
            if (operation == "decompress") //TODO убрать передачу флага
            {
                try
                {
                    fileStream = new FileStream(path, FileMode.Open);
                    var byteNumberOfBlocks = new byte[8];
                    fileStream.Read(byteNumberOfBlocks, 0, 8);
                    numberOfBlocks = BitConverter.ToInt64(byteNumberOfBlocks, 0);
                }
                catch (Exception)
                {
                    Console.WriteLine("Ошибка: неверный путь к файлу");
                    Environment.Exit(1);
                }
            }
            else
            {
                try
                {
                    fileStream = new FileStream(path, FileMode.Create);
                }
                catch (Exception)
                {
                    Console.WriteLine("Ошибка: неверный путь к файлу");
                    Environment.Exit(1);
                }
            }
        }
      
        public bool EndOfFile
        {
            get
            {
                if (fileStream.Position >= fileStream.Length)
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
        public long CurrentIndexOfBlock { get =>currentIndexOfBlock ;  }
        public long NumberOfBlocks { get => numberOfBlocks; }

        public void Dispose()
        {
            fileStream.Close();
        }      
        public void WriteBlock(byte[] block) 
        {
            var bytesCount = BitConverter.GetBytes(block.Length);
            List<byte> blocksToWrite = new List<byte>();
            blocksToWrite.AddRange(bytesCount);
            blocksToWrite.AddRange(block);
            fileStream.Write(blocksToWrite.ToArray(), 0, blocksToWrite.Count);            
        }
        public byte[] GetBlock()
        {
            byte[] byteLengthOfBlock = new byte[4];
            fileStream.Read(byteLengthOfBlock, 0, 4);
            var lengthOfBlock = BitConverter.ToInt32(byteLengthOfBlock, 0);          
            byte[] fileBlock = new byte[lengthOfBlock];
            fileStream.Read(fileBlock, 0, lengthOfBlock);
            currentIndexOfBlock++;
            return fileBlock;                               
        }
    
    }
}
