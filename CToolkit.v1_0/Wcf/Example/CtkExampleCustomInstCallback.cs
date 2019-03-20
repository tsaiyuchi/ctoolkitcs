using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_0.Wcf.Example
{
    public class CtkExampleCustomInstCallback : ICTkWcfDuplexOpCallback
    {




        #region ICTkWcfDuplexOpCallback

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
