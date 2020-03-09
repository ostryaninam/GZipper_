using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DataCollection
{
    public interface IBlockingCollection
    {
        bool IsCompleted { get; set; }
        bool IsEmpty { get; }
        int BoundedCapacity { get; }
        int Count { get; }
        AutoResetEvent CanTake { get; }
        AutoResetEvent CanAdd { get; }
        bool TryAdd(DataBlock block);
    }
}
