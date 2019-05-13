using CToolkit.v1_0.Protocol;
using CToolkit.v1_0.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CToolkit.v1_0.Wcf
{




    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CtkWcfDuplexTcpListenerInst : ICtkWcfDuplexOpService
    {
        //[ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ICTkWcfDuplexOpCallback))]
        //無法同時繼承並宣告ServiceContract


        public event EventHandler<CtkWcfDuplexEventArgs> evtReceiveMsg;


        public static CtkWcfDuplexTcpListener<T> NewInst<T>(T svrInst, NetTcpBinding _binding = null) where T : class, ICtkWcfDuplexOpService
        {
            if (_binding == null) _binding = new NetTcpBinding();
            return new CtkWcfDuplexTcpListener<T>(svrInst, _binding);
        }


        public static CtkWcfDuplexTcpListener<ICtkWcfDuplexOpService> NewDefault(NetTcpBinding _binding = null)
        {
            var svrInst = new CtkWcfDuplexTcpListenerInst();
            if (_binding == null) _binding = new NetTcpBinding();
            return NewInst<ICtkWcfDuplexOpService>(svrInst, _binding);
        }


        public void CtkSend(CtkWcfMessage msg)
        {
            var ea = new CtkWcfDuplexEventArgs();
            ea.WcfMsg = msg;
            ea.IsWcfNeedReturnMsg = false;
            this.OnReceiveMsg(ea);
        }

        public CtkWcfMessage CtkSendReply(CtkWcfMessage msg)
        {
            var ea = new CtkWcfDuplexEventArgs();
            ea.WcfMsg = msg;
            ea.IsWcfNeedReturnMsg = true;
            this.OnReceiveMsg(ea);
            return ea.WcfReturnMsg;
        }

        void OnReceiveMsg(CtkWcfDuplexEventArgs ea)
        {
            if (this.evtReceiveMsg == null) return;
            this.evtReceiveMsg(this, ea);
        }
    }


}
