using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileManagerLibrary.Implementations;
using DataCollection;

namespace Tests
{
    [TestClass]
    public class BlocksProducerConsumerTests
    {
        //[TestMethod]
        public void TestBlocksConsumer()
        {
            Gzip.BlocksConsumer blocksConsumer = new Gzip.BlocksConsumer(
                new CompressedFileFactory(@"C:\mults\Tangled1.gz").GetFileWriter(),
                new BlockingQueue<DataBlock>());
        }
        //[TestMethod]
        public void TestBlocksProducer()
        {
            Gzip.BlocksProducer blocksProducer = new Gzip.BlocksProducer(
                new SimpleFileFactory(@"C:\mults\Tangled.mkv", 1024 * 1024).GetFileReader(),
                new BlockingQueue<DataBlock>()
                );
        }
    }
}
