using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CToolkit.v1_0
{
    public class CtkObject
    {

        public static Object DataContractDeserialize(Type type, MemoryStream stream)
        {
            var dcSer = new DataContractSerializer(type);
            return dcSer.ReadObject(stream);
        }
        public static Object DataContractDeserialize(Type type, byte[] buffer)
        {
            using (var stm = new MemoryStream(buffer))
                return DataContractDeserialize(type, stm);
        }
        public static T DataContractDeserialize<T>(MemoryStream stream) { return (T)DataContractDeserialize(typeof(T), stream); }
        public static T DataContractDeserialize<T>(byte[] buffer) { return (T)DataContractDeserialize(typeof(T), buffer); }

        public static byte[] DataContractSerializeToByte<T>(T obj)
        {
            using (var stm = DataContractSerializeToStream(obj))
                return stm.GetBuffer();
        }
        public static MemoryStream DataContractSerializeToStream<T>(T obj)
        {
            DataContractSerializer dcSer = new DataContractSerializer(obj.GetType());
            MemoryStream memoryStream = new MemoryStream();
            dcSer.WriteObject(memoryStream, obj);
            memoryStream.Position = 0;
            return memoryStream;
        }



    }


}
