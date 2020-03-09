using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipLibrary
{
    interface IWorker
    {
        event EventHandler<Exception> ErrorOccured;

        event EventHandler CompleteEvent;
        void Start();
        void Stop();
    }
}
