using CToolkit.v1_1.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CToolkit.v1_1.Net
{
    public class CtkNonStopTcpListener : ICtkProtocolNonStopConnect, IDisposable
    {
        protected int m_IntervalTimeOfConnectCheck = 5000;
        public IPEndPoint localEP;
        TcpClient activeWorkClient;
        ManualResetEvent connectMre = new ManualResetEvent(true);
        ConcurrentQueue<TcpClient> m_tcpClientList = new ConcurrentQueue<TcpClient>();
        CtkTcpListenerEx m_tcpListener = null;
        Thread threadNonStopConnect;
        // = new BackgroundWorker();
        public CtkNonStopTcpListener() : base() { }

        public CtkNonStopTcpListener(IPEndPoint localEP)
        {
            this.localEP = localEP;
        }

        public CtkNonStopTcpListener(string localIp, int localPort)
        {
            IPAddress ipAddr;
            if (IPAddress.TryParse(localIp, out ipAddr))
                this.localEP = new IPEndPoint(ipAddr, localPort);
        }

        ~CtkNonStopTcpListener() { this.Dispose(false); }

        public ConcurrentQueue<TcpClient> TcpClientList { get { return this.m_tcpClientList; } }

        public void CleanDisconnect()
        {
            try
            {
                Monitor.TryEnter(this.TcpClientList, 1000);
                var list = new List<TcpClient>();
                TcpClient client = null;
                while (!this.TcpClientList.IsEmpty)
                {
                    if (!this.TcpClientList.TryDequeue(out client)) break;
                    if (client.Client != null && client.Connected)
                    {
                        list.Add(client);
                    }
                    else
                    {
                        CtkNetUtil.DisposeTcpClient(client);
                    }
                }

                foreach (var tc in list)
                    this.m_tcpClientList.Enqueue(tc);
            }
            catch (Exception ex) { CtkLog.Write(ex); }
            finally { Monitor.Exit(this.TcpClientList); }
        }
        public void CleanUntilLast()
        {
            var sourceList = this.m_tcpClientList;
            try
            {
                Monitor.TryEnter(sourceList, 1000);
                var list = new List<TcpClient>();
                TcpClient client = null;


                while (!sourceList.IsEmpty)
                {
                    if (!sourceList.TryDequeue(out client)) break;
                    if (sourceList.IsEmpty)
                    {
                        list.Add(client);
                    }
                    else
                    {
                        CtkNetUtil.DisposeTcpClient(client);
                    }
                }

                foreach (var tc in list)
                    sourceList.Enqueue(tc);
            }
            catch (Exception ex) { CtkLog.Write(ex); }
            finally { Monitor.Exit(sourceList); }
        }
        public void CleanExclude(TcpClient remindClient)
        {
            var sourceList = this.m_tcpClientList;
            try
            {
                Monitor.TryEnter(sourceList, 1000);
                var list = new List<TcpClient>();
                TcpClient client = null;
                while (!sourceList.IsEmpty)
                {
                    if (!sourceList.TryDequeue(out client)) break;
                    if (client == remindClient)
                    {
                        list.Add(client);
                    }
                    else
                    {
                        CtkNetUtil.DisposeTcpClient(client);
                    }
                }

                foreach (var tc in list)
                    sourceList.Enqueue(tc);
            }
            catch (Exception ex) { CtkLog.Write(ex); }
            finally { Monitor.Exit(sourceList); }
        }


        public int ConnectCount()
        {
            var cnt = 0;
            foreach (var tc in this.TcpClientList)
            {
                if (tc == null) continue;
                if (tc.Client == null) continue;
                if (!tc.Connected) continue;

                cnt++;
            }
            return cnt;
        }
        void EndConnectCallback(IAsyncResult ar)
        {
            var stateea = new CtkNonStopTcpStateEventArgs();
            var ctkBuffer = stateea.TrxMessageBuffer;
            try
            {
                // End the operation and display the received data on 
                // the console.
                var state = (CtkNonStopTcpListener)ar.AsyncState;
                stateea.Sender = state;
                var tcpClient = state.m_tcpListener.EndAcceptTcpClient(ar);
                stateea.workClient = tcpClient;
                this.m_tcpListener.BeginAcceptTcpClient(new AsyncCallback(EndConnectCallback), this);


                if (tcpClient.Client == null || !tcpClient.Connected)
                    throw new CtkException("連線失敗");

                this.TcpClientList.Enqueue(tcpClient);


                //呼叫他人不應影響自己運作, catch起來
                try { this.OnFirstConnect(stateea); }
                catch (Exception ex) { CtkLog.Write(ex); }

                NetworkStream stream = tcpClient.GetStream();
                stream.BeginRead(ctkBuffer.Buffer, 0, ctkBuffer.Buffer.Length, new AsyncCallback(EndReadCallback), stateea);

            }
            catch (Exception ex)
            {
                stateea.Message = ex.Message;
                stateea.Exception = ex;
                this.OnFailConnect(stateea);
            }
            finally { }
        }
        void EndReadCallback(IAsyncResult ar)
        {
            try
            {
                var tcpstate = (CtkNonStopTcpStateEventArgs)ar.AsyncState;
                var ctkBuffer = tcpstate.TrxMessageBuffer;
                var client = tcpstate.workClient;

                if (client == null || client.Client == null || !client.Connected) return;
                NetworkStream stream = client.GetStream();
                int bytesRead = stream.EndRead(ar);
                ctkBuffer.Length = bytesRead;

                //呼叫他人不應影響自己運作, catch起來
                try { this.OnDataReceive(tcpstate); }
                catch (Exception ex) { CtkLog.Write(ex); }

                stream.BeginRead(ctkBuffer.Buffer, 0, ctkBuffer.Buffer.Length, new AsyncCallback(EndReadCallback), tcpstate);

            }
            catch (Exception ex) { CtkLog.Write(ex); }
        }

        public void WriteBytes(byte[] buff, int offset, int length)
        {
            if (this.activeWorkClient == null) return;
            if (!this.activeWorkClient.Connected) return;

            var stm = this.activeWorkClient.GetStream();
            stm.Write(buff, offset, length);

        }

        #region ICtkProtocolNonStopConnect

        public event EventHandler<CtkProtocolEventArgs> EhDataReceive;
        public event EventHandler<CtkProtocolEventArgs> EhDisconnect;
        public event EventHandler<CtkProtocolEventArgs> EhErrorReceive;
        public event EventHandler<CtkProtocolEventArgs> EhFailConnect;
        public event EventHandler<CtkProtocolEventArgs> EhFirstConnect;

        public object ActiveWorkClient
        {
            get { return this.activeWorkClient; }
            set
            {
                if (!this.TcpClientList.Contains(value)) throw new ArgumentException("不可傳入別人的Tcp Client");
                this.activeWorkClient = value as TcpClient;
            }
        }

        public bool IsLocalReadyConnect { get { return this.m_tcpListener != null && this.m_tcpListener.Active; } }
        public bool IsNonStopRunning { get { return this.threadNonStopConnect != null && this.threadNonStopConnect.IsAlive; } }
        public bool IsOpenRequesting { get { return !this.connectMre.WaitOne(10); } }
        public bool IsRemoteConnected { get { return this.ConnectCount() > 0; } }

        public int IntervalTimeOfConnectCheck { get { return this.m_IntervalTimeOfConnectCheck; } set { this.m_IntervalTimeOfConnectCheck = value; } }

        public void AbortNonStopConnect()
        {
            if (this.threadNonStopConnect != null)
                this.threadNonStopConnect.Abort();
        }

        //用途是避免重複要求連線
        public void ConnectIfNo()
        {
            try
            {
                if (!Monitor.TryEnter(this, 1000)) return;//進不去先離開
                this.CleanDisconnect();
                if (!connectMre.WaitOne(10)) return;//連線中就離開
                this.connectMre.Reset();//先卡住, 不讓後面的再次進行連線


                if (this.m_tcpListener != null) return;
                //this.m_tcpListener.Stop();
                this.m_tcpListener = new CtkTcpListenerEx(this.localEP);
                this.m_tcpListener.Start();
                this.m_tcpListener.BeginAcceptTcpClient(new AsyncCallback(EndConnectCallback), this);
            }
            finally
            {
                this.connectMre.Set();
                Monitor.Exit(this);
            }
        }
        public void Disconnect()
        {

            if (this.threadNonStopConnect != null)
                this.threadNonStopConnect.Abort();

            foreach (var tc in this.TcpClientList)
            {
                if (tc == null) continue;
                try { tc.Close(); }
                catch (Exception ex) { CtkLog.Write(ex); }
            }

            if (this.m_tcpListener != null) this.m_tcpListener.Stop();

            //一旦結束就死了, 需要重new, 所以清掉event沒問題
            CtkEventUtil.RemoveEventHandlersOfOwnerByFilter(this, (dlgt) => true);


        }
        public void NonStopConnectAsyn()
        {
            AbortNonStopConnect();

            this.threadNonStopConnect = new Thread(new ThreadStart(delegate ()
            {

                while (!disposed)
                {
                    try
                    {
                        this.ConnectIfNo();
                    }
                    catch (Exception ex) { CtkLog.Write(ex); }

                    Thread.Sleep(this.IntervalTimeOfConnectCheck);

                }
            }));
            this.threadNonStopConnect.Start();

        }


        public void WriteMsg(CtkProtocolTrxMessage msg)
        {
            if (msg == null) throw new ArgumentException("Paramter cannot be null");
            var msgStr = msg.As<string>();
            if (msgStr != null)
            {
                var buff = Encoding.UTF8.GetBytes(msgStr);
                this.WriteBytes(buff, 0, buff.Length);
                return;
            }

            var msgBuffer = msg.As<CtkProtocolBufferMessage>();
            if (msgBuffer != null)
            {
                this.WriteBytes(msgBuffer.Buffer, msgBuffer.Offset, msgBuffer.Length);
                return;
            }

            var msgBytes = msg.As<byte[]>();
            if (msgBytes != null)
            {
                this.WriteBytes(msgBytes, 0, msgBytes.Length);
                return;
            }
        }

        void OnDataReceive(CtkProtocolEventArgs ea)
        {
            if (this.EhDataReceive == null) return;
            this.EhDataReceive(this, ea);
        }
        void OnDisconnect(CtkProtocolEventArgs tcpstate)
        {
            if (this.EhDisconnect == null) return;
            this.EhDisconnect(this, tcpstate);
        }
        void OnErrorReceive(CtkProtocolEventArgs ea)
        {
            if (this.EhErrorReceive == null) return;
            this.EhErrorReceive(this, ea);
        }
        void OnFailConnect(CtkProtocolEventArgs tcpstate)
        {
            if (this.EhFailConnect == null) return;
            this.EhFailConnect(this, tcpstate);
        }
        void OnFirstConnect(CtkProtocolEventArgs tcpstate)
        {
            if (this.EhFirstConnect == null) return;
            this.EhFirstConnect(this, tcpstate);
        }

        #endregion


        #region IDisposable
        // Flag: Has Dispose already been called?
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //

            this.DisposeSelf();

            disposed = true;
        }






        void DisposeSelf()
        {
            try { this.Disconnect(); }
            catch (Exception ex) { CtkLog.Write(ex); }
            CtkEventUtil.RemoveEventHandlersOfOwnerByFilter(this, (dlgt) => true);

        }

 


        #endregion
    }
}
