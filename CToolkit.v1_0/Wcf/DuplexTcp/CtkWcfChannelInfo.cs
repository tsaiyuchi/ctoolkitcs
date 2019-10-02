using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace CToolkit.v1_0.Wcf.DuplexTcp
{
    public class CtkWcfChannelInfo<T>
    {
        public OperationContext OpContext;
        public T Callback;
        public string SessionId;
        public IContextChannel Channel;
    }
}
