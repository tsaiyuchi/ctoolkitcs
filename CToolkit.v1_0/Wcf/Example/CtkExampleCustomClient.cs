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
    public class CtkExampleCustomClient : IDisposable
    {
        CtkWcfDuplexTcpClient<ICtkWcfDuplexOpService, CtkWcfDuplexTcpClient> client;
        CtkWcfDuplexTcpClient<ICtkExampleCustomListenerAdd, CtkWcfDuplexTcpClient> client1;
        CtkWcfDuplexTcpClient<ICtkExampleCustomListenerSubtract, CtkWcfDuplexTcpClient> client2;

        public const string ServerUri = @"net.tcp://localhost:9000/";


        ~CtkExampleCustomClient() { this.Dispose(false); }

        public void RunAsyn()
        {
            {
                this.client = CtkWcfDuplexTcpClient.CreateSingle<ICtkWcfDuplexOpService, CtkWcfDuplexTcpClient>();
                this.client.evtDataReceive += (ss, ee) =>
                {
                    var ea = ee as CtkWcfDuplexEventArgs;
                    CmdWrite("Client: " + ea.WcfMsg.DataObj + "");
                };
                this.client.Uri = ServerUri;
                this.client.ConnectIfNo();
            }

            {
                this.client1 = CtkWcfDuplexTcpClient.CreateSingle<ICtkExampleCustomListenerAdd, CtkWcfDuplexTcpClient>();
                this.client1.evtDataReceive += (ss, ee) =>
                {
                    var ea = ee as CtkWcfDuplexEventArgs;
                    CmdWrite("Client: " + ea.WcfMsg.DataObj + "");
                };
                this.client1.Uri = ServerUri;
                this.client1.EntryAddress = "Add";
                this.client1.ConnectIfNo();
            }

            {
                this.client2 = CtkWcfDuplexTcpClient.CreateSingle<ICtkExampleCustomListenerSubtract, CtkWcfDuplexTcpClient>();
                this.client2.evtDataReceive += (ss, ee) =>
                {
                    var ea = ee as CtkWcfDuplexEventArgs;
                    CmdWrite("Client: " + ea.WcfMsg.DataObj + "");
                };
                this.client2.Uri = ServerUri;
                this.client2.EntryAddress = "Sub";
                this.client2.ConnectIfNo();
            }

        }


        public void CmdWrite(string msg, params object[] obj)
        {
            if (msg != null)
            {
                Console.WriteLine();
                Console.WriteLine(msg, obj);
            }
            Console.Write(">");
        }

        public void CommandLine()
        {
            var cmd = "";
            do
            {
                CmdWrite(this.GetType().Name);
                cmd = Console.ReadLine();

                switch (cmd)
                {
                    case "send":
                        this.Send();
                        break;
                }


            } while (string.Compare(cmd, "exit", true) != 0);

            this.Close();

        }


        public void Send()
        {
            this.client.Channel.CtkSend("Hello, I am client");

            var rs = this.client1.Channel.Add(5, 2);
            this.client1.Channel.CtkSend("Hello, I am client1 -> " + rs);

            rs = this.client2.Channel.Subtract(5, 2);
            this.client2.Channel.CtkSend("Hello, I am client2 ->" + rs);
        }


        public void Close()
        {
            using (var obj = this.client)
                obj.Disconnect();


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
                this.DisposeManaged();
            }

            // Free any unmanaged objects here.
            //
            this.DisposeUnmanaged();

            this.DisposeSelf();

            disposed = true;
        }



        protected virtual void DisposeManaged()
        {
        }

        protected virtual void DisposeSelf()
        {
            this.Close();
        }

        protected virtual void DisposeUnmanaged()
        {

        }
        #endregion


    }
}
