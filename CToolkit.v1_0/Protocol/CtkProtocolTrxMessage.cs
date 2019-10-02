using CToolkit.v1_0.Wcf;
using CToolkit.v1_0.Wcf.DuplexTcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_0.Protocol
{
    public class CtkProtocolTrxMessage
    {
        public Object TrxMessage;
        public static CtkProtocolTrxMessage Create(Object msg) { return new CtkProtocolTrxMessage() { TrxMessage = msg }; }
        public static CtkProtocolTrxMessage Create(byte[] msg, int offset, int length) { return new CtkProtocolBufferMessage() { Buffer = msg, Offset = offset, Length = length }; }

        public static implicit operator CtkProtocolTrxMessage(byte[] msg) { return new CtkProtocolTrxMessage() { TrxMessage = msg }; }
        public static implicit operator CtkProtocolTrxMessage(string msg) { return new CtkProtocolTrxMessage() { TrxMessage = msg }; }
        public static implicit operator CtkProtocolTrxMessage(CtkProtocolBufferMessage msg) { return new CtkProtocolTrxMessage() { TrxMessage = msg }; }
        public static implicit operator CtkProtocolTrxMessage(CtkWcfMessage msg) { return new CtkProtocolTrxMessage() { TrxMessage = msg }; }

        public bool Is<T>() { return this.TrxMessage is T; }
        public T As<T>() where T : class { return this.TrxMessage as T; }
    }
}
