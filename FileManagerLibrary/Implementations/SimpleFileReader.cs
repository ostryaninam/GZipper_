using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FileManagerLibrary.Abstractions;

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
                    Console.WriteLine("Ошибка: размер файла меньше 1 Мб");
                }
                if (fileStream.Length % blockSize != 0)
                    numberOfBlocks++;
            }
            catch (Exception)
            {
                Console.WriteLine("Ошибка: неверный путь к файлу");
                Environment.Exit(1);
            }
        }
        public long CurrentIndexOfBlock => currentIndexOfBlock;

        public long NumberOfBlocks => numberOfBlocks;

        public bool EndOfFile => (fileStream.Position >= fileStream.Length);

        public byte[] ReadBlock()
        {
            byte[] fileBlock = new byte[blockSize];

            if (fileStream.Length >= blockSize)
            {
                try
                {
                    fileStream.Read(fileBlock, 0, blockSize);
                    currentIndexOfBlock++;
                    return fileBlock;
                }
                catch (EndOfStreamException)
                {
                    fileStream.Read(fileBlock, 0, (int)(fileStream.Length - fileStream.Position));
                    return fileBlock;
                }

            }
            else
            {
                fileStream.Read(fileBlock, 0, (int)fileStream.Length);
                return fileBlock;
            }
        }

        public void Dispose()
        {
            fileStream.Close();
        }

    }
}
