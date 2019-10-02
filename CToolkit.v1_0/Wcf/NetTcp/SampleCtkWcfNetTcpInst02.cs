using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace CToolkit.v1_0.Wcf.NetTcp
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class SampleCtkWcfNetTcpInst02 : SampleICtkWcfNetTcp0201, SampleICtkWcfNetTcp0202
    {
        public int Multiple(int a, int b)
        {
            return a * b;
        }

        public int Divide(int a, int b)
        {
            return a / b;
        }
    }
}
