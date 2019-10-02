using CToolkit.v1_0.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;

namespace CToolkit.v1_0.Wcf.DuplexTcp
{
    public class CtkWcfDuplexEventArgs : CtkProtocolEventArgs
    {
        public Object WcfChannel;
        public CtkWcfMessage WcfMsg;
        public CtkWcfMessage WcfReturnMsg;
        public bool IsWcfNeedReturnMsg;

    }
}
