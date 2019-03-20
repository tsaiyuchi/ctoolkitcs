using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace CToolkit.v1_0.Wcf.Example
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ICTkWcfDuplexOpCallback))]
    public interface ICtkExampleCustomListenerSubtract: ICtkWcfDuplexOpService
    {
        [OperationContract()]
        int Subtract(int a, int b);

    }
}
