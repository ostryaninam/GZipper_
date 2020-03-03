using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileManagerLibrary.Abstractions;
using FileManagerLibrary.Implementations;

namespace Tests
{
    [TestClass]
    public class FileReadersTests
    {
       [TestMethod] 
        public void TestSimpleFileReader()
        {
            var reader = new SimpleFileFactory(@"/GZipper_/Tests/Rihter_CLR-via-C.pdf", 1024*1024).GetFileReader();
            var countOfBlocks = 0;

            using (reader)
            {
                foreach (var block in reader)
                {                    
                    countOfBlocks++;
                }
            }

            Assert.AreEqual(reader.NumberOfBlocks, countOfBlocks);
        }

    }
}
