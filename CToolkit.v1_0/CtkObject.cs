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

        public static Object DataContractDeserialize(Type type, MemoryStream stream, IEnumerable<Type> types = null)
        {
            var seri = new DataContractSerializer(type);
            if (types != null) seri = new DataContractSerializer(type, types);
            return seri.ReadObject(stream);
        }
        public static Object DataContractDeserialize(Type type, byte[] buffer, IEnumerable<Type> types = null)
        {
            using (var stm = new MemoryStream(buffer))
                return DataContractDeserialize(type, stm, types);
        }
        public static T DataContractDeserialize<T>(MemoryStream stream, IEnumerable<Type> types = null) { return (T)DataContractDeserialize(typeof(T), stream, types); }
        public static T DataContractDeserialize<T>(byte[] buffer, IEnumerable<Type> types = null) { return (T)DataContractDeserialize(typeof(T), buffer, types); }

        public static byte[] DataContractSerializeToByte<T>(T obj, IEnumerable<Type> types = null)
        {
            using (var stm = DataContractSerializeToStream(obj))
                return stm.ToArray();//.GetBuffer();
        }
        public static MemoryStream DataContractSerializeToStream<T>(T obj, IEnumerable<Type> types = null)
        {
            var type = obj.GetType();
            var seri = new DataContractSerializer(type);
            if (types != null) seri = new DataContractSerializer(type, types);
            var memoryStream = new MemoryStream();
            seri.WriteObject(memoryStream, obj);
            memoryStream.Position = 0;
            return memoryStream;
        }



    }


}
