using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace CToolkit.v1_0.Wcf
{
    [Serializable]
    public class CtkWcfMessage
    {

        public String TypeName;
        public byte[] DataBytes;

        public void SetDataObj(object obj)
        {
            this.TypeName = obj.GetType().ToString();
            this.DataBytes = CtkObject.DataContractSerializeToByte(obj);

        }
        public Object GetDataObj()
        {
            var type = Type.GetType(this.TypeName);
            return CtkObject.DataContractDeserialize(type, this.DataBytes);
        }


        public static CtkWcfMessage Create(object obj)
        {
            var msg = new CtkWcfMessage();
            msg.SetDataObj(obj);
            return msg;
        }

        public static implicit operator CtkWcfMessage(string val)
        {
            var msg = new CtkWcfMessage();
            msg.SetDataObj(val);
            return msg;
        }


    }
}
