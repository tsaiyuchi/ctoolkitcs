using CToolkit.v1_0.Protocol;
using CToolkit.v1_0.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CToolkit.v1_0.Wcf
{


    /// <summary>
    /// //提供簡易訊息交換 & 收集 Channel
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    public class CtkWcfDuplexTcpListener<TService> : ICtkProtocolNonStopConnect
        where TService : ICtkWcfDuplexOpService
    {

        public Dictionary<string, Type> AddressMapInterface = new Dictionary<string, Type>();
        public string Uri;
        protected Binding binding;
        protected Dictionary<string, CtkWcfChannelInfo<ICTkWcfDuplexOpCallback>> channelMapper = new Dictionary<string, CtkWcfChannelInfo<ICTkWcfDuplexOpCallback>>();
        protected ServiceHost host;
        protected int m_IntervalTimeOfConnectCheck = 5000;
        protected TService serviceInstance;
        ICTkWcfDuplexOpCallback activeWorkClient;
        CtkCancelTask NonStopTask;
        public CtkWcfDuplexTcpListener(TService _svrInst, NetTcpBinding _binding)
        {
            this.serviceInstance = _svrInst;
            this.binding = _binding;
        }
        ~CtkWcfDuplexTcpListener() { this.Dispose(false); }



        public void CleanDisconnect()
        {
            var query = (from row in this.channelMapper
                         where row.Value.Channel.State > CommunicationState.Opened
                         select row).ToList();

            foreach (var row in query)
                this.channelMapper.Remove(row.Key);
        }

        public virtual void Close()
        {
            foreach (var chinfo in this.channelMapper)
            {
                var ch = chinfo.Value.Channel;
                ch.Abort();
                ch.Close();
            }

            if (this.host != null)
            {
                using (var obj = this.host)
                {
                    obj.Abort();
                    obj.Close();
                }
            }

            CtkEventUtil.RemoveEventHandlersFromOwningByFilter(this, (dlgt) => true);//關閉就代表此類別不用了
        }

        public List<ICTkWcfDuplexOpCallback> GetAllChannels()
        {
            this.CleanDisconnect();
            return (this.channelMapper.Select(row => row.Value.Callback)).ToList();
        }

        public T GetCallback<T>(string sessionId = null) where T : class, ICTkWcfDuplexOpCallback { return this.GetCallback(sessionId) as T; }

        public ICTkWcfDuplexOpCallback GetCallback(string sessionId = null)
        {
            this.CleanDisconnect();
            var oc = OperationContext.Current;
            if (sessionId == null)
                sessionId = oc.SessionId;
            if (this.channelMapper.ContainsKey(sessionId)) return this.channelMapper[sessionId].Callback;

            var chinfo = new CtkWcfChannelInfo<ICTkWcfDuplexOpCallback>();
            chinfo.OpContext = oc;
            chinfo.SessionId = sessionId;
            chinfo.Channel = oc.Channel;
            chinfo.Callback = oc.GetCallbackChannel<ICTkWcfDuplexOpCallback>();
            this.channelMapper[sessionId] = chinfo;
            return chinfo.Callback;
        }

        public virtual void NewHost()
        {
            var instance = this.serviceInstance;

            if (instance == null)
                this.host = new ServiceHost(typeof(TService), new Uri(this.Uri));
            else
                this.host = new ServiceHost(instance, new Uri(this.Uri));

            this.host.AddServiceEndpoint(typeof(TService), this.binding, "");


            if (this.AddressMapInterface != null)
            {
                foreach (var kv in this.AddressMapInterface)
                {
                    var ep = this.host.AddServiceEndpoint(kv.Value, this.binding, kv.Key);
                }
            }
        }

        public virtual void Open()
        {
            if (this.host == null) this.NewHost();
            this.host.Open();
        }

        void CleanHost()
        {
            //CtkEventUtil.RemoveEventHandlersFromOwningByFilter(this, (dlgt) => true);//不用清除自己的
            CtkEventUtil.RemoveEventHandlersFromOwningByTarget(this.host, this);
        }
     
        
        
        #region ICtkProtocolNonStopConnect


        public event EventHandler<CtkProtocolEventArgs> evtDataReceive;

        public event EventHandler<CtkProtocolEventArgs> evtDisconnect;

        public event EventHandler<CtkProtocolEventArgs> evtErrorReceive;

        public event EventHandler<CtkProtocolEventArgs> evtFailConnect;

        public event EventHandler<CtkProtocolEventArgs> evtFirstConnect;

        public object ActiveWorkClient { get { return this.activeWorkClient; } set { this.activeWorkClient = value as ICTkWcfDuplexOpCallback; } }

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
                this.CleanDisconnect();
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

        protected bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        public virtual void DisposeSelf()
        {
            this.Disconnect();
            this.Close();
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
