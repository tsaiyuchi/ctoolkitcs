using System;

namespace CToolkitCs.v1_1.Protocol
{
    public class CtkProtocolEventArgs : EventArgs
    {
        public Exception Exception;
        public string Message;
        public object Sender;
        public CtkProtocolTrxMessage TrxMessage;

    }
}
