using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileManagerLibrary.Abstractions;
using FileManagerLibrary.Implementations;

namespace Tests
{
    [TestClass]
    public class FileReadersTests
    {
       //[TestMethod] ok
        public void TestSimpleFileReader()
        {
            var reader = new SimpleFileFactory(@"C:\mults\Tangled.mkv", 1024*1024).GetFileReader();
            var count = 0;
            using (reader)
            {
                foreach (var block in reader)
                {                    
                    Console.WriteLine($"Block number {block.Index}");
                    count++;
                }
            }
            Console.WriteLine($"Count of blocks: {count}");
            Console.WriteLine($"Actual count of blocks: {reader.NumberOfBlocks}");
        }

       // [TestMethod]
        public void TestCompressedFileReader()
        {
            var reader = new CompressedFileFactory(@"C:\mults\Tangled.gz").GetFileReader();
            var count = 1;
            using (reader)
            {
                foreach (var block in reader)
                {
                    count++;
                }
            }
            Console.WriteLine($"Count of blocks: {count}");
            Console.WriteLine($"Actual count of blocks: {reader.NumberOfBlocks}");
        }
    }
}
