using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace CToolkitCs.v1_1.Wcf.NetTcp
{
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface SampleICtkWcfNetTcp0202
    {
        [OperationContract()]
        int Divide(int a, int b);
    }
}
