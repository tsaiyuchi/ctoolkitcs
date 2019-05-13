using CToolkit.v1_0;
using CToolkit.v1_0.Logging;
using CToolkit.v1_0.Net;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CToolkit.v1_0.Wcf.Example
{
    public class CtkExampleCustomListener : IDisposable
    {
        CtkWcfDuplexTcpListener<CtkExampleCustomInstListener> listenerCtk;

        public const string ServerUri = @"net.tcp://localhost:9000/";



        ~CtkExampleCustomListener() { this.Dispose(false); }

        public void RunAsyn()
        {
            this.listenerCtk = CtkWcfDuplexTcpListenerInst.NewInst(new CtkExampleCustomInstListener());
            this.listenerCtk.evtDataReceive += (ss, ee) =>
            {
                var ea = ee as CtkWcfDuplexEventArgs;
                CtkLog.InfoNs(this, "Client: " + ea.WcfMsg.TypeName + "");
            };

            var dict = new Dictionary<string, Type>();
            dict.Add("", typeof(ICtkWcfDuplexOpService));
            dict.Add("Add", typeof(ICtkExampleCustomListenerAdd));
            dict.Add("Sub", typeof(ICtkExampleCustomListenerSubtract));
            this.listenerCtk.Uri = ServerUri;
            this.listenerCtk.AddressMapInterface = dict;
            this.listenerCtk.ConnectIfNo();


        }




        public void Command(string cmd)
        {
            switch (cmd)
            {
                case "send":
                    this.Send();
                    break;
            }
        }


        public void Send()
        {
            this.listenerCtk.GetAllChannels().ForEach(row => row.CtkSend("Hello, I am server"));
        }


        public void Close()
        {
            using (var obj = this.listenerCtk)
                obj.Close();


        }


        #region IDisposable
        // Flag: Has Dispose already been called?
        protected bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
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





        protected virtual void DisposeSelf()
        {
            this.Close();
        }


        #endregion


    }
}
