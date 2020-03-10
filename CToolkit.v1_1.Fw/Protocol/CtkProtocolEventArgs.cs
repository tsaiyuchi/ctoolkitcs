using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace CToolkit.v1_1.Protocol
{
    public class CtkProtocolEventArgs : EventArgs
    {
        public Exception Exception;
        public string Message;
        public object Sender;
        public CtkProtocolTrxMessage TrxMessage;

    }
}
