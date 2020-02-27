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
            var reader = new SimpleFileFactory(@"C:\mults\Tangled.mkv", 1024*1024).GetFileReader();
            Gzip.BlocksProducer blocksProducer = new Gzip.BlocksProducer(
                reader,
                new BlockingQueue<DataBlock>()
                );
            ExceptionsHandling.ExceptionsHandler.Log($"Count of blocks: {reader.NumberOfBlocks}");
                
            blocksProducer.Start();
            blocksProducer.producingThread.Join();
        }
    }
}
