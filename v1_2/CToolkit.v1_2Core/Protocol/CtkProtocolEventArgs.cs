using System;

namespace CToolkit.v1_2Core.Protocol
{
    public class CtkProtocolEventArgs : EventArgs
    {
        public Exception Exception;
        public string Message;
        public object Sender;
        public CtkProtocolTrxMessage TrxMessage;

    }
}
