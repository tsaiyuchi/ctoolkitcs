using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace CToolkit.v1_0.Wcf
{


    /// <summary>
    /// 雙向(Duplex), 自行實作Callback收集
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TCallback"></typeparam>
    public class CtkWcfDuplexTcpListenerBasic<TService, TCallback> : IDisposable
    {
        public Dictionary<string, Type> AddressMapInterface = new Dictionary<string, Type>();
        public string Uri;
        protected Binding binding;
        protected Dictionary<string, CtkWcfChannelInfo<TCallback>> channelMapper = new Dictionary<string, CtkWcfChannelInfo<TCallback>>();
        protected ServiceHost host;
        protected TService serviceInstance;
        public CtkWcfDuplexTcpListenerBasic(Binding binding = null)
        {
            this.binding = binding;
        }

        public CtkWcfDuplexTcpListenerBasic(Binding binding, TService serviceInstance)
        {
            this.serviceInstance = serviceInstance;
            this.binding = binding;
        }

        ~CtkWcfDuplexTcpListenerBasic() { this.Dispose(false); }

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

        public List<TCallback> GetAllChannels()
        {
            this.CleanDisconnect();
            return (this.channelMapper.Select(row => row.Value.Callback)).ToList();
        }
        public T GetCallback<T>(string sessionId = null) where T : class, TCallback { return this.GetCallback(sessionId) as T; }

        public TCallback GetCallback(string sessionId = null)
        {
            this.CleanDisconnect();
            var oc = OperationContext.Current;
            if (sessionId == null)
                sessionId = oc.SessionId;
            if (this.channelMapper.ContainsKey(sessionId)) return this.channelMapper[sessionId].Callback;

            var chinfo = new CtkWcfChannelInfo<TCallback>();
            chinfo.OpContext = oc;
            chinfo.SessionId = sessionId;
            chinfo.Channel = oc.Channel;
            chinfo.Callback = oc.GetCallbackChannel<TCallback>();
            this.channelMapper[sessionId] = chinfo;
            return chinfo.Callback;
        }

        public virtual void Open()
        {
            if (this.host == null) this.NewHost();
            this.host.Open();
        }

        public virtual void NewHost()
        {
            var instance = this.serviceInstance;

            if (instance == null)
                this.host = new ServiceHost(typeof(TService), new Uri(this.Uri));
            else
                this.host = new ServiceHost(instance, new Uri(this.Uri));

            if (this.AddressMapInterface != null)
            {
                foreach (var kv in this.AddressMapInterface)
                {
                    var ep = this.host.AddServiceEndpoint(kv.Value, this.binding, kv.Key);
                }
            }
        }
        #region IDisposable

        protected bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void DisposeManaged()
        {

        }

        public virtual void DisposeSelf()
        {
            this.Close();
        }

        public virtual void DisposeUnManaged()
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
