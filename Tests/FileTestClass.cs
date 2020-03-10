using FileManagerLibrary.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Tests
{
    [TestClass]
    public class FileTestClass
    {
        [TestMethod]
        public void TestFileSimpleReader()
        {
            var debugPath = Directory.GetCurrentDirectory();
            var binPath = Path.GetDirectoryName(debugPath);
            var testsPath = Path.GetDirectoryName(binPath);
            var countOfBlocks = 0;

            using (var reader=new SimpleFileFactory($@"{testsPath}/Rihter_CLR-via-C.pdf", 1024 * 1024).GetFileReader())
            {
                foreach (var block in reader)
                {
                    countOfBlocks++;
                }
                Assert.AreEqual(reader.NumberOfBlocks, countOfBlocks);
            }

        }
        [TestMethod]
        public void TestFileCompressedReader()
        {
            var debugPath = Directory.GetCurrentDirectory();
            var binPath = Path.GetDirectoryName(debugPath);
            var testsPath = Path.GetDirectoryName(binPath);
            var countOfBlocks = 0;

            using (var reader = new CompressedFileFactory($@"{testsPath}/Rihter_CLR-via-C.gz").GetFileReader())
            {
                foreach (var block in reader)
                {
                    countOfBlocks++;
                }
                Assert.AreEqual(reader.NumberOfBlocks, countOfBlocks);
            }

        }
    }
}
