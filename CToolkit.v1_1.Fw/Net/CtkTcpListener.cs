using CToolkit.v1_1.Protocol;
using Newtonsoft.Json;
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
    public class CtkTcpListener : ICtkProtocolNonStopConnect, IDisposable
    {
        public Uri LocalUri;
        protected int m_IntervalTimeOfConnectCheck = 5000;
        bool IsReceiveLoop = false;
        ConcurrentQueue<TcpClient> m_tcpClientList = new ConcurrentQueue<TcpClient>();
        ManualResetEvent mreConnecting = new ManualResetEvent(true);
        ManualResetEvent mreReading = new ManualResetEvent(true);
        CtkTcpListenerEx myTcpListener = null;
        TcpClient myWorkClient;
        Thread threadNonStopConnect;
        // = new BackgroundWorker();
        public CtkTcpListener() : base() { }

        public CtkTcpListener(string localIp, int localPort)
        {
            if (localIp != null)
            {
                IPAddress.Parse(localIp);//Check format
                this.LocalUri = new Uri("net.tcp://" + localIp + ":" + localPort);
            }
        }

        ~CtkTcpListener() { this.Dispose(false); }

        public bool IsAutoRead { get; set; }
        public ConcurrentQueue<TcpClient> TcpClientList { get => m_tcpClientList; set => m_tcpClientList = value; }
        /// <summary>
        /// 開始讀取Socket資料, Begin 代表非同步.
        /// 用於 1. IsAutoRead被關閉, 每次讀取需自行執行;
        ///     2. 若連線還在, 但讀取異常中姒, 可以再度開始;
        /// </summary>
        public void BeginRead()
        {
            var myea = new CtkNonStopTcpStateEventArgs();
            myea.Sender = this;
            var client = this.ActiveWorkClient as TcpClient;
            myea.WorkTcpClient = client;
            var trxBuffer = myea.TrxMessageBuffer;
            var stream = client.GetStream();
            stream.BeginRead(trxBuffer.Buffer, 0, trxBuffer.Buffer.Length, new AsyncCallback(EndReadCallback), myea);
        }
        public void CleanExclude(TcpClient remindClient)
        {
            var sourceList = this.TcpClientList;
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
                        CtkNetUtil.DisposeTcpClientTry(client);
                    }
                }

                foreach (var tc in list)
                    sourceList.Enqueue(tc);
            }
            catch (Exception ex) { CtkLog.Write(ex); }
            finally { Monitor.Exit(sourceList); }
        }
        public void CleanInvalidClient()
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
                        CtkNetUtil.DisposeTcpClientTry(client);
                    }
                }

                foreach (var tc in list)
                    this.TcpClientList.Enqueue(tc);
            }
            catch (Exception ex) { CtkLog.Write(ex); }
            finally { Monitor.Exit(this.TcpClientList); }
        }
        public void CleanUntilLast()
        {
            var sourceList = this.TcpClientList;
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
                        CtkNetUtil.DisposeTcpClientTry(client);
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

        public int ReadLoop()
        {
            try
            {
                this.IsReceiveLoop = true;
                while (this.IsReceiveLoop && !this.disposed)
                {
                    this.ReadOnce();
                }
            }
            catch (Exception ex)
            {
                this.IsReceiveLoop = false;
                throw ex;//同步型作業, 直接拋出例外, 不用寫Log
            }
            return 0;
        }
        public void ReadLoopCancel()
        {
            this.IsReceiveLoop = false;
        }
        public int ReadOnce()
        {
            try
            {
                if (!Monitor.TryEnter(this, 1000)) return -1;//進不去先離開
                if (!this.mreReading.WaitOne(10)) return 0;//接收中先離開
                this.mreReading.Reset();//先卡住, 不讓後面的再次進行

                var ea = new CtkProtocolEventArgs()
                {
                    Sender = this,
                };

                ea.TrxMessage = new CtkProtocolBufferMessage(1518);
                var trxBuffer = ea.TrxMessage.ToBuffer();

                var stream = this.myWorkClient.GetStream();
                trxBuffer.Length = stream.Read(trxBuffer.Buffer, 0, trxBuffer.Buffer.Length);
                if (trxBuffer.Length == 0) return -1;
                this.OnDataReceive(ea);
            }
            catch (Exception ex)
            {
                this.OnErrorReceive(new CtkProtocolEventArgs() { Message = "Read Fail" });
                CtkNetUtil.DisposeTcpClientTry(this.myWorkClient);//執行出現例外, 先釋放
                throw ex;//同步型作業, 直接拋出例外, 不用寫Log
            }
            finally
            {
                this.mreReading.Set();//同步型的, 結束就可以Set
                if (Monitor.IsEntered(this)) Monitor.Exit(this);
            }
            return 0;
        }
        public void WriteBytes(byte[] buff, int offset, int length)
        {
            if (this.myWorkClient == null) return;
            if (!this.myWorkClient.Connected) return;

            var stm = this.myWorkClient.GetStream();
            stm.Write(buff, offset, length);

        }

        void EndConnectCallback(IAsyncResult ar)
        {
            var stateea = new CtkNonStopTcpStateEventArgs();
            var ctkBuffer = stateea.TrxMessageBuffer;
            try
            {
                // End the operation and display the received data on 
                // the console.
                var state = (CtkTcpListener)ar.AsyncState;
                stateea.Sender = state;
                var tcpClient = state.myTcpListener.EndAcceptTcpClient(ar);
                stateea.WorkTcpClient = tcpClient;
                this.myTcpListener.BeginAcceptTcpClient(new AsyncCallback(EndConnectCallback), this);


                if (tcpClient.Client == null || !tcpClient.Connected)
                    throw new CtkException("連線失敗");

                this.TcpClientList.Enqueue(tcpClient);


                //呼叫他人不應影響自己運作, catch起來
                try { this.OnFirstConnect(stateea); }
                catch (Exception ex) { CtkLog.Write(ex); }

                if (this.IsAutoRead)
                {
                    NetworkStream stream = tcpClient.GetStream();
                    stream.BeginRead(ctkBuffer.Buffer, 0, ctkBuffer.Buffer.Length, new AsyncCallback(EndReadCallback), stateea);
                }

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
                var client = tcpstate.WorkTcpClient;

                if (client == null || client.Client == null || !client.Connected) return;
                NetworkStream stream = client.GetStream();
                int bytesRead = stream.EndRead(ar);
                ctkBuffer.Length = bytesRead;

                //呼叫他人不應影響自己運作, catch起來
                try { this.OnDataReceive(tcpstate); }
                catch (Exception ex) { CtkLog.Write(ex); }

                if (this.IsAutoRead)
                    stream.BeginRead(ctkBuffer.Buffer, 0, ctkBuffer.Buffer.Length, new AsyncCallback(EndReadCallback), tcpstate);

            }
            catch (Exception ex) { CtkLog.Write(ex); }
        }


        #region ICtkProtocolConnect

        public event EventHandler<CtkProtocolEventArgs> EhDataReceive;
        public event EventHandler<CtkProtocolEventArgs> EhDisconnect;
        public event EventHandler<CtkProtocolEventArgs> EhErrorReceive;
        public event EventHandler<CtkProtocolEventArgs> EhFailConnect;
        public event EventHandler<CtkProtocolEventArgs> EhFirstConnect;

        [JsonIgnore]
        public object ActiveWorkClient
        {
            get { return this.myWorkClient; }
            set
            {
                if (!this.TcpClientList.Contains(value)) throw new ArgumentException("不可傳入別人的Tcp Client");
                this.myWorkClient = value as TcpClient;
            }
        }

        public bool IsLocalReadyConnect { get { return this.myTcpListener != null && this.myTcpListener.Active; } }
        public bool IsOpenRequesting { get { return !this.mreConnecting.WaitOne(10); } }
        public bool IsRemoteConnected { get { return this.ConnectCount() > 0; } }

        //用途是避免重複要求連線
        public int ConnectIfNo()
        {
            try
            {
                if (!Monitor.TryEnter(this, 1000)) return -1;//進不去先離開
                this.CleanInvalidClient();
                if (!mreConnecting.WaitOne(10)) return 0;//連線中就離開
                this.mreConnecting.Reset();//先卡住, 不讓後面的再次進行連線


                if (this.myTcpListener != null) return 0;//若要重新再聆聽, 請先清除Listener
                //this.m_tcpListener.Stop();
                this.myTcpListener = new CtkTcpListenerEx(IPAddress.Parse(this.LocalUri.Host), this.LocalUri.Port);
                this.myTcpListener.Start();
                var tcpClient = this.myTcpListener.AcceptTcpClient();
                this.TcpClientList.Enqueue(tcpClient);
                this.ActiveWorkClient = tcpClient;

                return 0;
            }
            finally
            {
                this.mreConnecting.Set();
                Monitor.Exit(this);
            }
        }
        public int ConnectIfNoAsyn()
        {
            try
            {
                if (!Monitor.TryEnter(this, 1000)) return -1;//進不去先離開
                this.CleanInvalidClient();
                if (!mreConnecting.WaitOne(10)) return 0;//連線中就離開
                this.mreConnecting.Reset();//先卡住, 不讓後面的再次進行連線


                if (this.myTcpListener != null) return 0;//若要重新再聆聽, 請先清除Listener
                //this.m_tcpListener.Stop();
                this.myTcpListener = new CtkTcpListenerEx(IPAddress.Parse(this.LocalUri.Host), this.LocalUri.Port);
                this.myTcpListener.Start();
                this.myTcpListener.BeginAcceptTcpClient(new AsyncCallback(EndConnectCallback), this);

                return 0;
            }
            finally
            {
                this.mreConnecting.Set();
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
                CtkNetUtil.DisposeTcpClientTry(tc);
            }
            if (this.myTcpListener != null) this.myTcpListener.Stop();
            this.myTcpListener = null;

            this.OnDisconnect(new CtkNonStopTcpStateEventArgs() { Message = "Disconnect method is executed" });
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

        #endregion


        #region ICtkProtocolNonStopConnect

        public int IntervalTimeOfConnectCheck { get { return this.m_IntervalTimeOfConnectCheck; } set { this.m_IntervalTimeOfConnectCheck = value; } }
        public bool IsNonStopRunning { get { return this.threadNonStopConnect != null && this.threadNonStopConnect.IsAlive; } }
        public void AbortNonStopRun()
        {
            if (this.threadNonStopConnect != null)
                this.threadNonStopConnect.Abort();
        }
        public void NonStopRunAsyn()
        {
            AbortNonStopRun();

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
            CtkEventUtil.RemoveEventHandlersOfOwnerByFilter(this, (dlgt) => true);

        }

        #endregion
    }
}
