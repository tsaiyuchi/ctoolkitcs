using CToolkit.v1_1.Protocol;
using CToolkit.v1_1.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CToolkit.v1_1.Wcf.NetTcp
{

    /// <summary>
    /// 尚不完整, 除了自己的專案以外, 盡量不要用, 僅供範例參考
    /// </summary>
    public class CtkWcfNetTcpClient<TService> : IDisposable
    {


        public TService Channel;
        public ChannelFactory<TService> ChannelFactory;
        public string Uri;
        public string EntryAddress;
        public NetTcpBinding Binding;
        public bool IsWcfConnected { get { return this.ChannelFactory != null && this.ChannelFactory.State <= CommunicationState.Opened; } }

        ~CtkWcfNetTcpClient() { this.Dispose(false); }


        public CtkWcfNetTcpClient(NetTcpBinding _binding = null)
        {
            this.Binding = _binding;
        }

        public CtkWcfNetTcpClient(string uri, NetTcpBinding _binding = null)
        {
            this.Uri = uri;
            this.Binding = _binding;
        }



        public virtual void WcfConnect(Action<ChannelFactory<TService>> beforeOpen = null)
        {
            if (string.IsNullOrEmpty(this.Uri)) throw new ArgumentNullException("The Uri must has value");
            if (this.IsWcfConnected) return;
            try
            {
                if (!Monitor.TryEnter(this, 1000)) return;//進不去先離開

                var address = this.Uri;
                if (this.EntryAddress != null) address = Path.Combine(this.Uri, this.EntryAddress);
                var endpointAddress = new EndpointAddress(address);

                if (this.Binding == null) this.Binding = new NetTcpBinding();
                this.ChannelFactory = new ChannelFactory<TService>(this.Binding, endpointAddress);

                if (beforeOpen != null) beforeOpen.Invoke(this.ChannelFactory);//一次性使用的東西, 可以不寫event


                this.Channel = this.ChannelFactory.CreateChannel();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }







        #region IDispose

        protected bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void DisposeSelf()
        {

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
