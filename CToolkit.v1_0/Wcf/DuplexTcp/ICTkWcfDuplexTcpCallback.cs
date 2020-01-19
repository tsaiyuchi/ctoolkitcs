using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace CToolkit.v1_0.Wcf.DuplexTcp
{
    public interface ICTkWcfDuplexTcpCallback 
    {
        //[OperationContract()]
        event EventHandler<CtkWcfDuplexEventArgs> EhReceiveMsg;

        [OperationContract(IsOneWay = true)]
        void CtkSend(CtkWcfMessage msg);

        [OperationContract()]
        CtkWcfMessage CtkSendReply(CtkWcfMessage msg);


    }
}
