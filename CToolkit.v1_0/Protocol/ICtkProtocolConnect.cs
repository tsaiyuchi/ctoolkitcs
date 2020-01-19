using CToolkit.v1_0.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_0.Protocol
{
    public interface ICtkProtocolConnect
    {
        bool IsLocalReadyConnect { get; }//Local連線成功=遠端連線成功
        bool IsRemoteConnected { get; }
        bool IsOpenRequesting { get; }//用途是避免重複要求連線

        void ConnectIfNo();
        void Disconnect();


        object ActiveWorkClient { get; set; }//可要求變更Active Work
        void WriteMsg(CtkProtocolTrxMessage msg);


        event EventHandler<CtkProtocolEventArgs> EhFirstConnect;
        event EventHandler<CtkProtocolEventArgs> EhFailConnect;
        event EventHandler<CtkProtocolEventArgs> EhDisconnect;
        event EventHandler<CtkProtocolEventArgs> EhDataReceive;
        event EventHandler<CtkProtocolEventArgs> EhErrorReceive;

    }
}
