using CToolkitCs.v1_2Core.Protocol;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CToolkitCs.v1_2Core.Net.SocketTx
{
    public class CtkSocketUdp : CtkSocket
    {


        public override Socket CreateSocket() { return new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); }

    }

}
