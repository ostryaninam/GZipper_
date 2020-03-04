using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCollection;

namespace Gzip
{
    interface IBlockGZipper
    {
        byte[] Execute(byte[] block);
    }
}
