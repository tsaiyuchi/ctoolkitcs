using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_0.Wcf
{
    public class CtkWcfMessage
    {


        public Object DataObj;



        public static implicit operator CtkWcfMessage(string val) { return new CtkWcfMessage() { DataObj = val }; }
    }
}
