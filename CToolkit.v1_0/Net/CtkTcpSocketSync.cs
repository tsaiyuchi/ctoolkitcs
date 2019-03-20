using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;



namespace CToolkit.v1_0.Net
{
    public class CtkTcpSocketSync : IDisposable
    {
        public bool isActively = false;
        public IPEndPoint local;
        public IPEndPoint remote;
        protected Socket m_connSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        bool m_isWaitReceive = false;
        Socket m_workSocket;
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
        public void Connect() { this.Connect(this.isActively); }

        public void Connect(bool isAct)
        {
            this.isActively = isAct;
            if (isAct)
            {
                if (this.local != null && !this.ConnSocket.IsBound)
                    this.ConnSocket.Bind(this.local);
                if (this.remote == null)
                    throw new CtkException("remote field can not be null");

                this.ConnSocket.Connect(this.remote);
                this.WorkSocket = this.ConnSocket;
            }
            else
            {
                if (this.local == null)
                    throw new Exception("local field can not be null");
                if (!this.ConnSocket.IsBound)
                    this.ConnSocket.Bind(this.local);



                this.ConnSocket.Listen(100);
                this.WorkSocket = this.ConnSocket.Accept();
            }
        }
        public void Disconnect() { CtkNetUtil.DisposeSocket(this.m_connSocket); }

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

        #region ReceiveData

        public event Action<CtkTcpSocketSync, CtkTcpSocketStateEventArgs> eventReceiveData;
        public void OnReceiveData(CtkTcpSocketStateEventArgs state)
        {
            if (this.eventReceiveData == null)
                return;

            this.eventReceiveData(this, state);
        }

        #endregion



        #region Dispose

        bool disposed = false;
        public void Dispose()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        public void DisposeManaged()
        {
        }

        public void DisposeSelf()
        {
            this.IsWaitTcpReceive = false;
            CtkNetUtil.DisposeSocket(this.m_connSocket);
            CtkNetUtil.DisposeSocket(this.WorkSocket);
        }

        public void DisposeUnManaged()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any managed objects here.
                this.DisposeManaged();
            }

            // Free any unmanaged objects here.
            //
            this.DisposeUnManaged();
            this.DisposeSelf();
            disposed = true;
        }
        #endregion





    }
}

