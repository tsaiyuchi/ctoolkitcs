using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkitCs.v1_1.Numeric
{
    [Serializable]
    public class CtkCudafyCannotUseException : CtkException
    {

        public CtkCudafyCannotUseException() { }
        public CtkCudafyCannotUseException(string message) : base(message) { }
    }
}
