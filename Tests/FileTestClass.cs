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
        public void TestFileReader()
        {
            var debugPath = Directory.GetCurrentDirectory();
            var binPath = Path.GetDirectoryName(debugPath);
            var testsPath = Path.GetDirectoryName(binPath);
            var reader = new SimpleFileFactory($@"{testsPath}/Rihter_CLR-via-C.pdf", 1024 * 1024).GetFileReader();
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
