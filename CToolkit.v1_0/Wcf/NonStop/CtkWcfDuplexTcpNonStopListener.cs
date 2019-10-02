using CToolkit.v1_0.Protocol;
using CToolkit.v1_0.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CToolkit.v1_0.Wcf.DuplexTcp
{
    public class CtkWcfDuplexTcpNonStopListener<TService> : CtkWcfDuplexTcpListener<TService>, ICtkProtocolNonStopConnect
         where TService : ICtkWcfDuplexTcpService
    {
        protected int m_IntervalTimeOfConnectCheck = 5000;
        ICTkWcfDuplexTcpCallback activeWorkClient;
        CtkCancelTask NonStopTask;



        public CtkWcfDuplexTcpNonStopListener(TService _svrInst, NetTcpBinding _binding = null) : base(_svrInst, _binding)
        {
        }

        ~CtkWcfDuplexTcpNonStopListener() { this.Dispose(false); }


        #region ICtkProtocolNonStopConnect


        public event EventHandler<CtkProtocolEventArgs> evtDataReceive;

        public event EventHandler<CtkProtocolEventArgs> evtDisconnect;

        public event EventHandler<CtkProtocolEventArgs> evtErrorReceive;

        public event EventHandler<CtkProtocolEventArgs> evtFailConnect;

        public event EventHandler<CtkProtocolEventArgs> evtFirstConnect;

        public object ActiveWorkClient { get { return this.activeWorkClient; } set { this.activeWorkClient = value as ICTkWcfDuplexTcpCallback; } }

        public int IntervalTimeOfConnectCheck { get { return this.m_IntervalTimeOfConnectCheck; } set { this.m_IntervalTimeOfConnectCheck = value; } }

        public bool IsLocalReadyConnect { get { return this.host != null && this.host.State <= CommunicationState.Opened; } }

        public bool IsNonStopRunning { get { return this.NonStopTask != null && this.NonStopTask.Task.Status < TaskStatus.RanToCompletion; } }

        public bool IsOpenRequesting { get { try { return Monitor.TryEnter(this, 10); } finally { Monitor.Exit(this); } } }

        public bool IsRemoteConnected { get { return this.GetAllChannels().Count > 0; } }
        public void AbortNonStopConnect()
        {
            if (this.NonStopTask != null)
            {
                using (var obj = this.NonStopTask)
                    obj.Cancel();
            }
        }

        public void ConnectIfNo()
        {
            if (this.IsLocalReadyConnect) return;
            try
            {
                if (!Monitor.TryEnter(this, 1000)) return;//進不去先離開
                if (this.IsLocalReadyConnect) return;
                this.CleanDisconnected();
                this.CleanHost();
                this.NewHost();

                this.host.Opened += (ss, ee) =>
                {
                    var ea = new CtkWcfDuplexEventArgs();
                    //ea.WcfChannel = this.GetCallback();//Listener(or call Host, Service) 開啟後, 並沒有Channel連線進來
                    this.OnFirstConnect(ea);
                };


                this.serviceInstance.evtReceiveMsg += (ss, ee) =>
                {
                    var ea = ee;
                    ea.WcfChannel = this.GetCallback();
                    this.OnDataReceive(ea);
                };

                this.host.Closed += (ss, ee) =>
                {
                    var ea = new CtkWcfDuplexEventArgs();
                    //ea.WcfChannel = this.GetCallback();//Listerner關閉, 會關閉所有Channel, 並沒有特定哪一個
                    this.OnDisconnect(ea);
                };
                this.Open();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void Disconnect()
        {
            this.AbortNonStopConnect();
            this.Close();
        }

        public void NonStopConnectAsyn()
        {
            AbortNonStopConnect();

            this.NonStopTask = CtkCancelTask.Run((ct) =>
            {
                while (!this.disposed && !ct.IsCancellationRequested)
                {
                    ct.ThrowIfCancellationRequested();
                    try
                    {
                        this.ConnectIfNo();
                    }
                    catch (Exception ex) { CtkLog.Write(ex); }
                    Thread.Sleep(this.IntervalTimeOfConnectCheck);
                }

            });
        }



        /// <summary>
        /// 只支援 CtkWcfMessage
        /// </summary>
        /// <param name="msg"></param>
        public void WriteMsg(CtkProtocolTrxMessage msg)
        {
            var wcfmsg = msg.As<CtkWcfMessage>();
            if (wcfmsg != null)
            {
                this.activeWorkClient.CtkSend(msg.As<CtkWcfMessage>());
                return;
            }
            throw new ArgumentException("No support type");
        }

        void OnDataReceive(CtkProtocolEventArgs ea)
        {
            if (this.evtDataReceive == null) return;
            this.evtDataReceive(this, ea);
        }
        void OnDisconnect(CtkProtocolEventArgs tcpstate)
        {
            if (this.evtDisconnect == null) return;
            this.evtDisconnect(this, tcpstate);
        }
        void OnErrorReceive(CtkProtocolEventArgs ea)
        {
            if (this.evtErrorReceive == null) return;
            this.evtErrorReceive(this, ea);
        }
        void OnFailConnect(CtkProtocolEventArgs tcpstate)
        {
            if (this.evtFailConnect == null) return;
            this.evtFailConnect(this, tcpstate);
        }
        void OnFirstConnect(CtkProtocolEventArgs tcpstate)
        {
            if (this.evtFirstConnect == null) return;
            this.evtFirstConnect(this, tcpstate);
        }


        #endregion


        #region IDisposable

        public override void DisposeSelf()
        {
            this.Disconnect();
            base.DisposeSelf();
        }

        #endregion

    }
}
