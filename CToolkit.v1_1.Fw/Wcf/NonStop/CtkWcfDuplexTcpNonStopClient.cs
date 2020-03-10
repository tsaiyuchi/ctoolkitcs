using CToolkit.v1_1.Protocol;
using CToolkit.v1_1.Threading;
using CToolkit.v1_1.Wcf.DuplexTcp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CToolkit.v1_1.Wcf.NonStop
{

    /// <summary>
    /// 尚不完整, 除了自己的專案以外, 盡量不要用
    /// </summary>
    public class CtkWcfDuplexTcpNonStopClient<TService, TCallback> : CtkWcfDuplexTcpClient<TService, TCallback>
        , IDisposable
        , ICtkProtocolNonStopConnect
        where TService : ICtkWcfDuplexTcpService//Server提供的, 必須是interface
        where TCallback : ICTkWcfDuplexTcpCallback//提供給Server呼叫的
    {
        CtkCancelTask NonStopTask;
        protected int m_IntervalTimeOfConnectCheck = 5000;


        public CtkWcfDuplexTcpNonStopClient(TCallback _callbackInst, NetTcpBinding _binding = null) : base(_callbackInst, _binding)
        {

        }




        #region ICtkProtocolNonStopConnect


        public event EventHandler<CtkProtocolEventArgs> EhDataReceive;

        public event EventHandler<CtkProtocolEventArgs> EhDisconnect;

        public event EventHandler<CtkProtocolEventArgs> EhErrorReceive;

        public event EventHandler<CtkProtocolEventArgs> EhFailConnect;

        public event EventHandler<CtkProtocolEventArgs> EhFirstConnect;

        public object ActiveWorkClient { get { return this.Channel; } set { this.Channel = (TService)value; } }

        public bool IsLocalReadyConnect { get { return this.IsWcfConnected; } }

        public bool IsNonStopRunning { get { return this.NonStopTask != null && this.NonStopTask.Task.Status < TaskStatus.RanToCompletion; } }

        public bool IsOpenRequesting { get { try { return Monitor.TryEnter(this, 10); } finally { Monitor.Exit(this); } } }

        public bool IsRemoteConnected { get { return this.ChannelFactory.State == CommunicationState.Opened; } }

        public int IntervalTimeOfConnectCheck { get { return this.m_IntervalTimeOfConnectCheck; } set { this.m_IntervalTimeOfConnectCheck = value; } }

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
            this.WcfConnect(cf =>
            {
                cf.Opened += (ss, ee) =>
                {
                    var ea = new CtkWcfDuplexEventArgs();
                    ea.WcfChannel = this.Channel;
                    this.OnFirstConnect(ea);
                };
                cf.Closed += (ss, ee) =>
                {
                    var ea = new CtkWcfDuplexEventArgs();
                    ea.WcfChannel = this.Channel;
                    this.OnDisconnect(ea);
                };
            });

        }

        public void Disconnect()
        {
            this.AbortNonStopConnect();
            if (this.ChannelFactory != null)
            {
                using (var obj = this.ChannelFactory)
                {
                    obj.Abort();
                    obj.Close();
                }
            }

            CtkEventUtil.RemoveEventHandlersFromOwningByFilter(this, (dlgt) => true);

        }

        public void NonStopConnectAsyn()
        {
            AbortNonStopConnect();

            this.NonStopTask = CtkCancelTask.RunOnce((ct) =>
            {
                while (!this.disposed && !ct.IsCancellationRequested)
                {
                    ct.ThrowIfCancellationRequested();
                    try
                    {
                        this.ConnectIfNo();
                    }
                    catch (Exception ex) { CtkLog.Write(ex); }
                    Thread.Sleep(this.m_IntervalTimeOfConnectCheck);
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
                this.Channel.CtkSend(msg.As<CtkWcfMessage>());
                return;
            }
            throw new ArgumentException("No support type");
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


        #region IDispose

        public override void DisposeSelf()
        {
            this.Disconnect();
            base.DisposeSelf();
        }

        #endregion
    }






}
