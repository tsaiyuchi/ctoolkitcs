using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace CToolkit.v1_0.Net
{
    public class CtkTcpSocketStateEventArgs
    {
        public object sender;
        public Socket workSocket;
        public byte[] buffer;
        public int dataSize;
    }
}
