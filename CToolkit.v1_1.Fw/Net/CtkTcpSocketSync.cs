using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using CToolkit.v1_1.Protocol;

namespace CToolkit.v1_1.Net
{
    public class CtkTcpSocketSync : ICtkProtocolConnect, IDisposable
    {
        public bool IsActively = false;
        public IPEndPoint Local;
        public IPEndPoint Remote;

        protected Socket m_connSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        protected bool m_isOpenRequesting = false;
        protected bool m_isWaitReceive = false;
        protected Socket m_workSocket;

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


            lock (this) this.m_isOpenRequesting = true;

            if (isAct)
            {
                if (this.Local != null && !this.ConnSocket.IsBound)
                    this.ConnSocket.Bind(this.Local);
                if (this.Remote == null)
                    throw new CtkException("remote field can not be null");

                this.ConnSocket.Connect(this.Remote);
                this.WorkSocket = this.ConnSocket;
            }
            else
            {
                if (this.Local == null)
                    throw new Exception("local field can not be null");
                if (!this.ConnSocket.IsBound)
                    this.ConnSocket.Bind(this.Local);


                this.ConnSocket.Listen(100);
                this.WorkSocket = this.ConnSocket.Accept();
            }

            lock (this) this.m_isOpenRequesting = false;

        }

        public void ReceiveRepeat()
        {
            try
            {
                this.IsWaitTcpReceive = true;
                while (this.IsWaitTcpReceive && !this.disposed)
                {
                    var state = new CtkTcpSocketStateEventArgs()
                    {
                        sender = this,
                        workSocket = this.WorkSocket,//Actively socket 為連線的socket本身
                        buffer = new byte[1518]
                    };

                    state.dataSize = state.workSocket.Receive(state.buffer, 0, state.buffer.Length, SocketFlags.None);
                    if (state.dataSize == 0)
                        break;
                    this.OnReceiveData(state);
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
        #endregion


        #region ReceiveData

        public event Action<CtkTcpSocketSync, CtkTcpSocketStateEventArgs> EhDatReceivea;
        public void OnReceiveData(CtkTcpSocketStateEventArgs state)
        {
            if (this.EhDatReceivea == null)
                return;

            this.EhDatReceivea(this, state);
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
            this.IsWaitTcpReceive = false;
            CtkNetUtil.DisposeSocket(this.WorkSocket);
            CtkNetUtil.DisposeSocket(this.m_connSocket);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

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

