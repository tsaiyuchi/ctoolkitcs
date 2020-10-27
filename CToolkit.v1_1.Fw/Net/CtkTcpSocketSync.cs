using CToolkit.v1_1.Protocol;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CToolkit.v1_1.Net
{
    public class CtkTcpSocketSync : ICtkProtocolConnect, IDisposable
    {
        public bool IsActively = false;
        public Uri LocalUri;
        public Uri RemoteUri;
        protected Socket m_connSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        protected bool m_isOpenRequesting = false;
        protected bool m_isWaitReceive = false;
        protected Socket m_workSocket;

        ~CtkTcpSocketSync() { this.Dispose(false); }
        public Socket ConnSocket { get { return m_connSocket; } }

        public bool IsWaitTcpReceive

        {

            get { return m_isWaitReceive; }

            set { lock (this) { m_isWaitReceive = value; } }

        }
        public Socket WorkSocket

        {

            get { return m_workSocket; }

            set { lock (this) { m_workSocket = value; } }

        }
        public bool CheckConnectStatus()
        {
            var socket = this.m_connSocket;
            if (socket == null) return false;
            if (!socket.Connected) return false;
            return !(socket.Poll(1000, SelectMode.SelectRead) && (socket.Available == 0));
        }

        public void ConnectIfNo(bool isAct)
        {
            this.IsActively = isAct;
            if (this.IsOpenRequesting || this.IsRemoteConnected) return;

            try
            {
                var canLock = false;
                Monitor.TryEnter(this, 3000, ref canLock);
                if (!canLock) throw new CtkException("Cannot enter lock");
                this.m_isOpenRequesting = true;

                if (isAct)
                {

                    if (this.LocalUri != null && !this.ConnSocket.IsBound)
                        this.ConnSocket.Bind(CtkNetUtil.ToIPEndPoint(this.LocalUri));
                    if (this.RemoteUri == null)
                        throw new CtkException("remote field can not be null");
                    this.ConnSocket.Connect(CtkNetUtil.ToIPEndPoint(this.RemoteUri));
                    this.WorkSocket = this.ConnSocket;
                }
                else
                {
                    if (this.LocalUri == null)
                        throw new Exception("local field can not be null");
                    if (!this.ConnSocket.IsBound)
                        this.ConnSocket.Bind(CtkNetUtil.ToIPEndPoint(this.LocalUri));
                    this.ConnSocket.Listen(100);
                    this.WorkSocket = this.ConnSocket.Accept();
                }
            }
            finally
            {
                this.m_isOpenRequesting = false;
                Monitor.Exit(this);
            }
        }

        public void ReceiveRepeat()

        {

            try

            {

                this.IsWaitTcpReceive = true;

                while (this.IsWaitTcpReceive && !this.disposed)

                {

                    var ea = new CtkProtocolEventArgs()

                    {

                        Sender = this,

                    };

                    ea.TrxMessage = new CtkProtocolBufferMessage(1518);

                    var trxBuffer = ea.TrxMessage.ToBuffer();



                    trxBuffer.Length = this.WorkSocket.Receive(trxBuffer.Buffer, 0, trxBuffer.Buffer.Length, SocketFlags.None);

                    if (trxBuffer.Length == 0) break;

                    this.OnDataReceive(ea);

                }

            }

            finally

            {

                if (this.ConnSocket != this.WorkSocket) CtkNetUtil.DisposeSocket(this.WorkSocket);

            }







        }

        #region ICtkProtocolConnect



        public event EventHandler<CtkProtocolEventArgs> EhDataReceive;

        public event EventHandler<CtkProtocolEventArgs> EhDisconnect;

        public event EventHandler<CtkProtocolEventArgs> EhErrorReceive;

        public event EventHandler<CtkProtocolEventArgs> EhFailConnect;

        public event EventHandler<CtkProtocolEventArgs> EhFirstConnect;

        public object ActiveWorkClient { get { return this.WorkSocket; } set { this.WorkSocket = value as Socket; } }

        public bool IsLocalReadyConnect { get { return this.m_connSocket != null && this.m_connSocket.Connected; } }

        public bool IsOpenRequesting { get { return this.m_isOpenRequesting; } }

        public bool IsRemoteConnected { get { return this.WorkSocket != null && this.WorkSocket.Connected; } }

        public void ConnectIfNo() { this.ConnectIfNo(this.IsActively); }

        public void Disconnect() { CtkNetUtil.DisposeSocket(this.m_connSocket); }

        public void WriteMsg(CtkProtocolTrxMessage msg)

        {

            try

            {

                Monitor.Enter(this);

                var buffer = msg.ToBuffer();

                this.WorkSocket.Send(buffer.Buffer, buffer.Offset, buffer.Length, SocketFlags.None);

            }

            finally { if (Monitor.IsEntered(this)) Monitor.Exit(this); }



        }

        protected void OnDataReceive(CtkProtocolEventArgs ea)

        {

            if (this.EhDataReceive == null) return;

            this.EhDataReceive(this, ea);

        }

        #endregion








        #region Dispose



        bool disposed = false;

        public void Dispose()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        public void DisposeSelf()
        {
            //this.IsWaitTcpReceive = false;
            this.m_isWaitReceive = false;
            try { CtkNetUtil.DisposeSocket(this.WorkSocket); }
            catch (Exception ex) { CtkLog.WarnNs(this, ex); }

            try { CtkNetUtil.DisposeSocket(this.m_connSocket); }
            catch (Exception ex) { CtkLog.WarnNs(this, ex); }

            CtkEventUtil.RemoveEventHandlersOfOwnerByFilter(this, (dlgt) => true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                // Free any managed objects here.
            }
            // Free any unmanaged objects here.
            //
            this.DisposeSelf();
            disposed = true;
        }



        #endregion











    }

}





