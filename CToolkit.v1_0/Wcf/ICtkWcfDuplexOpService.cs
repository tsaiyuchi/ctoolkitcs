using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CToolkit.v1_0.Wcf
{

    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ICTkWcfDuplexOpCallback))]
    public interface ICtkWcfDuplexOpService
    {


        //[OperationContract()]
        event EventHandler<CtkWcfDuplexEventArgs> evtReceiveMsg;

        [OperationContract(IsOneWay = true)]
        void CtkSend(CtkWcfMessage msg);

        [OperationContract()]
        CtkWcfMessage CtkSendReply(CtkWcfMessage msg);

    }



}
