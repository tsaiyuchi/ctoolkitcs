using System;

namespace CToolkit.v1_1.Protocol
{
    public interface ICtkProtocolConnect
    {
        /// <summary>
        /// 若為Client, Local連線成功=遠端連線成功.
        /// 若為Server, 開啟聆聽即為準備好連線
        /// </summary>
        bool IsLocalReadyConnect { get; }
        /// <summary>
        /// 遠端真的連線成功
        /// </summary>
        bool IsRemoteConnected { get; }
        /// <summary>
        /// 在準備開啟連時中設為true, 用途是避免重複要求連線
        /// </summary>
        bool IsOpenRequesting { get; }

        int ConnectIfNo();
        void Disconnect();


        /// <summary>
        /// 目前啟用中的連線, 可以是任何物件.
        /// 有可能作為Server有多個 Clients, 正在操作中的是哪個.
        /// 作為 Client 就只有一個 Server, 正在操作中的也是同一個.
        /// </summary>
        object ActiveWorkClient { get; set; }//可要求變更Active Work
        void WriteMsg(CtkProtocolTrxMessage msg);


        event EventHandler<CtkProtocolEventArgs> EhFirstConnect;
        event EventHandler<CtkProtocolEventArgs> EhFailConnect;
        event EventHandler<CtkProtocolEventArgs> EhDisconnect;
        event EventHandler<CtkProtocolEventArgs> EhDataReceive;
        event EventHandler<CtkProtocolEventArgs> EhErrorReceive;

    }
}
