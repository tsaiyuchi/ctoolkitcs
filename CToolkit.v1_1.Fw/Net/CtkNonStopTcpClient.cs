using CToolkit.v1_1.Logging;
using CToolkit.v1_1.Net;
using CToolkit.v1_1.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CToolkit.v1_1.Net
{
    public class CtkNonStopTcpClient : ICtkProtocolNonStopConnect, IDisposable
    {

        public Uri LocalUri;
        public Uri RemoteUri;
        protected int m_IntervalTimeOfConnectCheck = 5000;
        TcpClient m_activeClient;
        ManualResetEvent mreIsConnecting = new ManualResetEvent(true);
        Thread threadNonStopConnect;

        public CtkNonStopTcpClient() : base() { }

        public CtkNonStopTcpClient(Uri remote)
        {
            this.RemoteUri = remote;
        }

        public CtkNonStopTcpClient(string remoteIp, int remotePort, string localIp = null, int localPort = 0)
        {
            if (!string.IsNullOrEmpty(remoteIp))
            {
                IPAddress.Parse(remoteIp);//Check format
                this.RemoteUri = new Uri("net.tcp://" + remoteIp + ":" + remotePort);
            }

            if (!string.IsNullOrEmpty(localIp))
            {
                IPAddress.Parse(localIp);//Check format
                this.LocalUri = new Uri("net.tcp://" + localIp + ":" + localPort);
            }
        }

        ~CtkNonStopTcpClient() { this.Dispose(false); }

        [JsonIgnore]
        protected TcpClient ActiveClient { get { lock (this) return m_activeClient; } set { lock (this) m_activeClient = value; } }



        /// <summary>
        /// Remember use stream.Flush() to force data send, Tcp Client always write data into buffer.
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void WriteBytes(byte[] buff, int offset, int length)
        {
            if (this.ActiveClient == null) return;
            if (!this.ActiveClient.Connected) return;


            try
            {
                var stm = this.ActiveClient.GetStream();
                stm.Write(buff, offset, length);
                stm.Flush();

            }
            catch (Exception ex)
            {
                //資料寫入錯誤, 普遍是斷線造成, 先中斷連線清除資料
                this.Disconnect();
                CtkLog.WarnNs(this, ex);
            }
            //stm.BeginWrite(buff, offset, length, new AsyncCallback((ar) =>
            //{
            //    //CtkLog.WriteNs(this, "" + ar.IsCompleted);
            //}), this);


        }

        void ClientEndConnectCallback(IAsyncResult ar)
        {
            var myea = new CtkNonStopTcpStateEventArgs();
            var trxBuffer = myea.TrxMessageBuffer;
            try
            {
                //Lock使用在短碼保護, 例如: 保護一個變數的get/set
                //Monitor使用在保護一段代碼

                Monitor.Enter(this);//一定要等到進去
                var state = (CtkNonStopTcpClient)ar.AsyncState;
                var client = state.ActiveClient;
                client.EndConnect(ar);

                myea.Sender = state;
                myea.workClient = client;
                if (!ar.IsCompleted || client.Client == null || !client.Connected)
                {
                    throw new CtkException("Connection Fail");
                }

                //呼叫他人不應影響自己運作, catch起來
                try { this.OnFirstConnect(myea); }
                catch (Exception ex) { CtkLog.WarnNs(this, ex); }

                var stream = client.GetStream();
                stream.BeginRead(trxBuffer.Buffer, 0, trxBuffer.Buffer.Length, new AsyncCallback(EndReadCallback), myea);
            }
            //catch (SocketException ex) { }
            catch (Exception ex)
            {
                //失敗就中斷連線, 清除
                this.Disconnect();
                myea.Message = ex.Message;
                myea.Exception = ex;
                this.OnFailConnect(myea);
                CtkLog.WarnNs(this, ex);
            }
            finally
            {
                this.mreIsConnecting.Set();
                Monitor.Exit(this);
            }
        }
        void EndReadCallback(IAsyncResult ar)
        {
            //var stateea = (CtkNonStopTcpStateEventArgs)ar.AsyncState;
            var myea = (CtkNonStopTcpStateEventArgs)ar.AsyncState;
            try
            {
                var client = myea.workClient;
                if (!ar.IsCompleted || client == null || client.Client == null || !client.Connected)
                {
                    throw new CtkException("Read Fail");
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
            //catch (IOException ex) { CtkLog.Write(ex); }
            catch (Exception ex)
            {
                //讀取失敗, 中斷連線(會呼叫 OnDisconnect), 不需要呼叫 OnFailConnect
                this.Disconnect();
                myea.Message = ex.Message;
                myea.Exception = ex;
                this.OnErrorReceive(myea);//但要呼叫 OnErrorReceive
                CtkLog.WarnNs(this, ex);
            }
        }



        #region ICtkProtocolNonStopConnect


        public event EventHandler<CtkProtocolEventArgs> EhDataReceive;
        public event EventHandler<CtkProtocolEventArgs> EhDisconnect;
        public event EventHandler<CtkProtocolEventArgs> EhErrorReceive;
        public event EventHandler<CtkProtocolEventArgs> EhFailConnect;
        public event EventHandler<CtkProtocolEventArgs> EhFirstConnect;

        [JsonIgnore]
        public object ActiveWorkClient { get { return this.ActiveClient; } set { if (this.ActiveClient != value) throw new ArgumentException("不可傳入Active Client"); } }
        public int IntervalTimeOfConnectCheck { get { return this.m_IntervalTimeOfConnectCheck; } set { this.m_IntervalTimeOfConnectCheck = value; } }
        public bool IsLocalReadyConnect { get { return this.IsRemoteConnected; } }//Local連線成功=遠端連線成功
        public bool IsNonStopRunning { get { return this.threadNonStopConnect != null && this.threadNonStopConnect.IsAlive; } }
        public bool IsOpenRequesting { get { return !this.mreIsConnecting.WaitOne(10); } }
        public bool IsRemoteConnected { get { return CtkNetUtil.IsConnected(this.ActiveClient); } }


        public void AbortNonStopConnect()
        {
            if (this.threadNonStopConnect != null)
                this.threadNonStopConnect.Abort();
        }

        //用途是避免重複要求連線
        public int ConnectIfNo()
        {
            try
            {
                if (!Monitor.TryEnter(this, 1000)) return -1;//進不去先離開

                if (!mreIsConnecting.WaitOne(10)) return 0;//連線中就離開
                this.mreIsConnecting.Reset();//先卡住, 不讓後面的再次進行連線

                //在Lock後才判斷, 避免判斷無連線後, 另一邊卻連線好了
                if (this.ActiveClient != null && this.ActiveClient.Connected) return 0;//連線中直接離開
                if (this.ActiveClient != null)
                {
                    var workClient = this.ActiveClient;
                    try
                    {
                        if (workClient.GetStream() != null)
                            using (var stm = workClient.GetStream()) stm.Close();
                    }
                    catch (Exception) { }

                    try { workClient.Close(); }
                    catch (Exception ex) { CtkLog.Write(ex); }

                    this.ActiveClient = null;
                }


                IPAddress ip = null;
                if (IPAddress.TryParse(this.LocalUri.Host, out ip))
                {
                    this.ActiveClient = new TcpClient(new IPEndPoint(ip, this.LocalUri.Port));
                }
                else this.ActiveClient = new TcpClient();
                //this.activeWorkClient = this.LocalUri == null ? new TcpClient() : new TcpClient(LocalUri.Host, LocalUri.Port);
                this.ActiveClient.NoDelay = true;
                this.ActiveClient.BeginConnect(this.RemoteUri.Host, this.RemoteUri.Port, new AsyncCallback(ClientEndConnectCallback), this);

                return 0;
            }
            catch (Exception ex)
            {
                //若中間有失效, 解除Event鎖
                this.mreIsConnecting.Set();
                //停止連線
                this.Disconnect();
                throw ex;
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
            CtkNetUtil.DisposeTcpClient(this.ActiveClient);

            var myea = new CtkNonStopTcpStateEventArgs();
            myea.Message = "Disconnect";
            this.OnDisconnect(myea);
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

        #endregion




        #region Event

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
            //斷線不用清除Event, 但Dispsoe需要
            CtkEventUtil.RemoveEventHandlersOfOwnerByFilter(this, (dlgt) => true);
        }

        #endregion

    }
}
