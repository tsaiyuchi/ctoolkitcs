using System;

namespace CToolkitCs.v1_2Core.Hal
{
    public class CtkHalException : CtkException
    {
        public CtkHalException() : base() { }
        public CtkHalException(string message) : base(message) { }
        public CtkHalException(string message, Exception innerException) : base(message, innerException) { }


        public CtkHalException(Type type, string method, string message)
            : base(string.Format("{0}.{1}.{2}", type.FullName, method, message)) { }
        public CtkHalException(Type type, string method, string message, Exception innerException)
            : base(string.Format("{0}.{1}.{2}", type.FullName, method, message), innerException) { }

    }
}
