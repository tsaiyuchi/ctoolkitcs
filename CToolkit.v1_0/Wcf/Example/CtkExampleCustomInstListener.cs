using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace CToolkit.v1_0.Wcf.Example
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CtkExampleCustomInstListener : ICtkWcfDuplexOpService, ICtkExampleCustomListenerAdd, ICtkExampleCustomListenerSubtract
    {

        public int Add(int a, int b)
        {
            return a + b;
        }

        public int Subtract(int a, int b)
        {
            return a - b;
        }



        #region ICtkWcfDuplexOpService

        public event EventHandler<CtkWcfDuplexEventArgs> evtReceiveMsg;
        protected virtual void OnCtkReceiveMessage(CtkWcfDuplexEventArgs ea)
        {
            if (this.evtReceiveMsg == null) return;
            this.evtReceiveMsg(this, ea);
        }

        public void CtkSend(CtkWcfMessage msg)
        {
            this.OnCtkReceiveMessage(new CtkWcfDuplexEventArgs() { WcfMsg = msg });
        }

        public CtkWcfMessage CtkSendReply(CtkWcfMessage msg)
        {
            var ea = new CtkWcfDuplexEventArgs() { WcfMsg = msg };
            this.OnCtkReceiveMessage(ea);
            return ea.WcfReturnMsg;
        }

        #endregion
    }
}
