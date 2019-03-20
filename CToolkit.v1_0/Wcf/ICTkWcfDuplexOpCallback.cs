using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace CToolkit.v1_0.Wcf
{
    public interface ICTkWcfDuplexOpCallback 
    {
        //[OperationContract()]
        event EventHandler<CtkWcfDuplexEventArgs> evtReceiveMsg;

        [OperationContract(IsOneWay = true)]
        void CtkSend(CtkWcfMessage msg);

        [OperationContract()]
        CtkWcfMessage CtkSendReply(CtkWcfMessage msg);


    }
}
