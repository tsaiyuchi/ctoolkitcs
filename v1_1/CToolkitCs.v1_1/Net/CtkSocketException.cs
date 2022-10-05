using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CToolkitCs.v1_1.Net
{
    public class CtkSocketException : CtkException
    {
        public CtkSocketException() : base() { }
        public CtkSocketException(String message) : base(message) { }
    }
}
