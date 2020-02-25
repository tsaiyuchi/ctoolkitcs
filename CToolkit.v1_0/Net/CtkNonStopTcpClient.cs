using CToolkit.v1_0.Logging;
using CToolkit.v1_0.Net;
using CToolkit.v1_0.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CToolkit.v1_0.Net
{
    public class CtkNonStopTcpClient : ICtkProtocolNonStopConnect, IDisposable
    {

        public IPEndPoint localEP;
        public IPEndPoint remoteEP;
        protected int m_IntervalTimeOfConnectCheck = 5000;
        TcpClient activeWorkClient;
        ManualResetEvent mreIsConnecting = new ManualResetEvent(true);
        Thread threadNonStopConnect;// = new BackgroundWorker();
        public CtkNonStopTcpClient() : base() { }
        public CtkNonStopTcpClient(IPEndPoint remoteEP)
        {
            this.remoteEP = remoteEP;
        }
        public CtkNonStopTcpClient(string remoteIp, int remotePort, string localIp = null, int localPort = 0)
        {
            IPAddress remoteIpAddr;
            if (IPAddress.TryParse(remoteIp, out remoteIpAddr))
                this.remoteEP = new IPEndPoint(remoteIpAddr, remotePort);

            IPAddress localIpAddr;
            if (IPAddress.TryParse(localIp, out localIpAddr))
                this.localEP = new IPEndPoint(localIpAddr, localPort);
        }

        ~CtkNonStopTcpClient() { this.Dispose(false); }

        public void WriteBytes(byte[] buff, int offset, int length)
        {
            if (this.activeWorkClient == null) return;
            if (!this.activeWorkClient.Connected) return;

            var stm = this.activeWorkClient.GetStream();
            stm.BeginWrite(buff, offset, length, new AsyncCallback((ar) =>
            {
                //CtkLog.WriteNs(this, "" + ar.IsCompleted);
            }), this);


        }

        void ClientEndConnectCallback(IAsyncResult ar)
        {
            var myea = new CtkNonStopTcpStateEventArgs();
            var trxBuffer = myea.TrxMessageBuffer;
            try
            {
                Monitor.Enter(this);//一定要等到進去
                var state = (CtkNonStopTcpClient)ar.AsyncState;
                var client = state.activeWorkClient;
                client.EndConnect(ar);

                myea.Sender = state;
                myea.workClient = client;
                if (!ar.IsCompleted || client.Client == null || !client.Connected)
                {
                    myea.Message = "Connection Fail";
                    this.OnFailConnect(myea);
                    return;
                }

                //呼叫他人不應影響自己運作, catch起來
                try { this.OnFirstConnect(myea); }
                catch (Exception ex) { CtkLog.Write(ex); }

                var stream = client.GetStream();
                stream.BeginRead(trxBuffer.Buffer, 0, trxBuffer.Buffer.Length, new AsyncCallback(EndReadCallback), myea);


            }
            catch (SocketException ex)
            {
                myea.Message = ex.Message;
                myea.Exception = ex;
                this.OnFailConnect(myea);
            }
            catch (Exception ex)
            {
                CtkLog.Write(ex, CtkLoggerEnumLevel.Warn);
            }
            finally
            {
                this.mreIsConnecting.Set();
                Monitor.Exit(this);
            }
        }


        void EndReadCallback(IAsyncResult ar)
        {
            try
            {
                //var stateea = (CtkNonStopTcpStateEventArgs)ar.AsyncState;
                var myea = (CtkNonStopTcpStateEventArgs)ar.AsyncState;
                var client = myea.workClient;
                if(! ar.IsCompleted || client == null || client.Client == null || !client.Connected)
                {
                    myea.Message = "Read Fail";
                    return;
                }

                var ctkBuffer = myea.TrxMessageBuffer;
                NetworkStream stream = client.GetStream();
                int bytesRead = stream.EndRead(ar);
                ctkBuffer.Length = bytesRead;
                //呼叫他人不應影響自己運作, catch起來
                try { this.OnDataReceive(myea); }
                catch (Exception ex) { CtkLog.Write(ex); }
                stream.BeginRead(ctkBuffer.Buffer, 0, ctkBuffer.Buffer.Length, new AsyncCallback(EndReadCallback), myea);

            }
            catch (IOException ex) { CtkLog.Write(ex); }
            catch (Exception ex) { CtkLog.Write(ex); }
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
            set { if (this.activeWorkClient != value) throw new ArgumentException("不可傳入Active Client"); }
        }

        public int IntervalTimeOfConnectCheck { get { return this.m_IntervalTimeOfConnectCheck; } set { this.m_IntervalTimeOfConnectCheck = value; } }
        public bool IsLocalReadyConnect { get { return this.IsRemoteConnected; } }//Local連線成功=遠端連線成功
        public bool IsNonStopRunning { get { return this.threadNonStopConnect != null && this.threadNonStopConnect.IsAlive; } }
        public bool IsOpenRequesting { get { return !this.mreIsConnecting.WaitOne(10); } }
        public bool IsRemoteConnected { get { return this.activeWorkClient == null ? false : this.activeWorkClient.Connected; } }
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

                //在Lock後才判斷, 避免判斷無連線後, 另一邊卻連線好了
                if (this.activeWorkClient != null && this.activeWorkClient.Connected) return;//連線中直接離開
                if (!mreIsConnecting.WaitOne(10)) return;//連線中就離開
                this.mreIsConnecting.Reset();//先卡住, 不讓後面的再次進行連線

                if (this.activeWorkClient != null)
                {
                    var workClient = this.activeWorkClient;
                    try
                    {
                        if (workClient.GetStream() != null)
                            using (var stm = workClient.GetStream()) stm.Close();
                    }
                    catch (Exception) { }

                    try { workClient.Close(); }
                    catch (Exception ex) { CtkLog.Write(ex); }

                    this.activeWorkClient = null;
                }
                this.activeWorkClient = localEP == null ? new TcpClient() : new TcpClient(localEP);
                this.activeWorkClient.NoDelay = true;
                this.activeWorkClient.BeginConnect(this.remoteEP.Address, this.remoteEP.Port, new AsyncCallback(ClientEndConnectCallback), this);

            }
            finally { Monitor.Exit(this); }

        }

        public void Disconnect()
        {
            if (this.threadNonStopConnect != null)
            {
                this.threadNonStopConnect.Abort();
                this.threadNonStopConnect = null;
            }
            CtkNetUtil.DisposeTcpClient(this.activeWorkClient);

            //一旦結束就死了, 需要重new, 所以清掉event沒問題
            CtkEventUtil.RemoveEventHandlersFromOwningByFilter(this, (dlgt) => true);

        }

        public void NonStopConnectAsyn()
        {
            AbortNonStopConnect();

            this.threadNonStopConnect = new Thread(new ThreadStart(delegate ()
            {
                //TODO: 重啟時, 會有執行緒被中止的狀況
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

            throw new ArgumentException("Cannot support this type: " + msg.ToString());




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
        }

        #endregion

    }
}
