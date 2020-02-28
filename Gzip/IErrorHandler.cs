using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gzip
{
    public delegate void ErrorHandler(object sender, Exception ex);
    public interface IErrorHandler
    {
        event ErrorHandler ErrorOccured;
    }
}
