using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CToolkitCs.v1_1.Wcf.NetTcp
{

    [DataContract]
    [KnownType(typeof(SampleCtkWcfNetTcpMsg))]
    public class SampleCtkWcfNetTcpReq
    {


        public Object Data = new SampleCtkWcfNetTcpMsg();
    }
}
