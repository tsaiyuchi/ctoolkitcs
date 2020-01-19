using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CToolkit.v1_0.Wcf.DuplexTcp
{

    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ICTkWcfDuplexTcpCallback))]
    public interface ICtkWcfDuplexTcpService
    {


        //[OperationContract()]
        event EventHandler<CtkWcfDuplexEventArgs> EhReceiveMsg;

        [OperationContract(IsOneWay = true)]
        void CtkSend(CtkWcfMessage msg);

        [OperationContract()]
        CtkWcfMessage CtkSendReply(CtkWcfMessage msg);

    }



}
