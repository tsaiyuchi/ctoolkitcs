using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_0
{
    public class CtkObject
    {



        public static T  DataContractSerialization<T>(T obj)
        {
            System.Runtime.Serialization.DataContractSerializer dcSer = new System.Runtime.Serialization.DataContractSerializer(obj.GetType());
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();

            dcSer.WriteObject(memoryStream, obj);
            memoryStream.Position = 0;

            T newObject = (T)dcSer.ReadObject(memoryStream);
            return newObject;
        }

    }
}
