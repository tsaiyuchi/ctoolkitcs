using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_1
{
    [Serializable]
    public class CtkException : Exception
    {
        public CtkException() : base() { }
        public CtkException(string message) : base(message) { }
        protected CtkException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        public CtkException(string message, Exception innerException) : base(message, innerException) { }


        public CtkException(Type type, string method, string message)
            : base(string.Format("{0}.{1}.{2}", type.FullName, method, message)) { }
        public CtkException(Type type, string method, string message, Exception innerException)
            : base(string.Format("{0}.{1}.{2}", type.FullName, method, message), innerException) { }
    }
}