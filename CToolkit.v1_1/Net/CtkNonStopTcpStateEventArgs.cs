using CToolkit.v1_1.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace CToolkit.v1_1.Net
{
    public class CtkNonStopTcpStateEventArgs : CtkProtocolEventArgs
    {
        public TcpClient workClient;


        public CtkProtocolBufferMessage TrxMessageBuffer
        {
            get
            {
                if (this.TrxMessage == null) this.TrxMessage = new CtkProtocolBufferMessage();
                if (!this.TrxMessage.Is<CtkProtocolBufferMessage>()) throw new InvalidOperationException("TrxMessage is not Buffer");
                return this.TrxMessage.As<CtkProtocolBufferMessage>();
            }
            set { this.TrxMessage = value; }
        }



        public void WriteMsg(byte[] buff, int offset, int length)
        {
            if (this.workClient == null) return;
            if (!this.workClient.Connected) return;

            var stm = this.workClient.GetStream();
            stm.Write(buff, offset, length);

        }
        public void WriteMsg(byte[] buff, int length) { this.WriteMsg(buff, 0, length); }
        public void WriteMsg(String msg)
        {
            var buff = Encoding.UTF8.GetBytes(msg);
            this.WriteMsg(buff, 0, buff.Length);
        }
    }
}
