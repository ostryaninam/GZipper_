using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gzip
{
    public delegate bool EndHasCome(object sender);
    interface IEnding
    {
        event EndHasCome EndEvent;
    }
}
