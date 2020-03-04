using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gzip
{
    public delegate bool CompleteEventHandler(object sender);
    interface ICompleted
    {
        event CompleteEventHandler CompleteEvent;
    }
}
