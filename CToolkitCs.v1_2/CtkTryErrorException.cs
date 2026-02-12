using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkitCs.v1_2
{
    public class CtkTryErrorException : CtkException
    {
        public List<Exception> Exceptions = new List<Exception>();

        public CtkTryErrorException() : base() { }
        public CtkTryErrorException(string message) : base(message) { }
        public CtkTryErrorException(string message, Exception innerException) : base(message, innerException) { }


        public CtkTryErrorException(Type type, string method, string message)
            : base(string.Format("{0}.{1}.{2}", type.FullName, method, message)) { }
        public CtkTryErrorException(Type type, string method, string message, Exception innerException)
            : base(string.Format("{0}.{1}.{2}", type.FullName, method, message), innerException) { }
    }
}