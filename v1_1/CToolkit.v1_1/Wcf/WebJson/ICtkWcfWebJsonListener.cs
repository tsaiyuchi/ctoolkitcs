using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CToolkit.v1_1.Wcf.WebJson
{

    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface ICtkWcfWebJsonListener
    {
        [OperationContract()]
        void Capture();

    }



}
