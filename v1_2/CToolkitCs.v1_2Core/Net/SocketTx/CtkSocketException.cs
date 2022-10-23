using System;
using System.Collections.Generic;
using System.Text;

namespace CToolkitCs.v1_2Core.Net.SocketTx
{
    public class CtkSocketException : CtkException
    {
        public CtkSocketException() : base() { }
        public CtkSocketException(String message) : base(message) { }
    }
}
