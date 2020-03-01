using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace CToolkit.v1_1.Wcf.NetTcp
{
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface SampleICtkWcfNetTcp0201
    {
        [OperationContract()]
        int Multiple(int a, int b);

    }
}
