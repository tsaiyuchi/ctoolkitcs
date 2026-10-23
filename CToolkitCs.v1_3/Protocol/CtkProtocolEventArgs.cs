using System;
using System.Net;

namespace CToolkitCs.v1_3.Protocol
{
    public class CtkProtocolEventArgs : EventArgs
    {
        public Exception Exception;
        public string Message;
        /// <summary> 參數發送者 </summary>
        public object Sender;
        public CtkProtocolTrxMessage TrxMessage;
    
    }
}
