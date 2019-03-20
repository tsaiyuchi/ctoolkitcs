using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_0.Protocol
{
    public class CtkProtocolBufferMessage
    {
        public CtkProtocolBufferMessage() { }
        public CtkProtocolBufferMessage(int bufferSize)
        {
            this.Buffer = new byte[bufferSize];
        }

        public byte[] Buffer = new byte[1024];
        public int Offset;
        public int Length;


        public string GetString(Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            return encoding.GetString(this.Buffer, this.Offset, this.Length);
        }


        public static implicit operator CtkProtocolBufferMessage(byte[] data) { return new CtkProtocolBufferMessage() { Buffer = data, Offset = 0, Length = data.Length }; }
    }
}
