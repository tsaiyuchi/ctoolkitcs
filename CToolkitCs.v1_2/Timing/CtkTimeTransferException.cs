using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkitCs.v1_2.Timing
{
    public class CtkTimeTransferException : CtkException
    {
        public CtkTimeTransferException() : base() { }
        public CtkTimeTransferException(string message) : base(message) { }
        public CtkTimeTransferException(string message, Exception innerException) : base(message, innerException) { }


        public CtkTimeTransferException(Type type, string method, string message)
            : base(string.Format("{0}.{1}.{2}", type.FullName, method, message)) { }
        public CtkTimeTransferException(Type type, string method, string message, Exception innerException)
            : base(string.Format("{0}.{1}.{2}", type.FullName, method, message), innerException) { }
    }
}