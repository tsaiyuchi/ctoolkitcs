using CToolkitCs.v1_1.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;

namespace CToolkitCs.v1_1.Wcf.DuplexTcp
{
    public class CtkWcfDuplexEventArgs : CtkProtocolEventArgs
    {
        public Object WcfChannel;
        public CtkWcfMessage WcfMsg;
        public CtkWcfMessage WcfReturnMsg;
        public bool IsWcfNeedReturnMsg;

    }
}
