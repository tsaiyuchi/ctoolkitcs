using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace CToolkitCs.v1_1.Wcf.DuplexTcp
{
    public class CtkWcfChannelInfo<T>
    {
        public OperationContext OpContext;
        public T Callback;
        public string SessionId;
        public IContextChannel Channel;
    }
}
