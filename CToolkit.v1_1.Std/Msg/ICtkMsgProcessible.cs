using System;
using System.Collections.Generic;
using System.Text;

namespace CToolkit.v1_1.Msg
{
    public interface ICtkMsgProcessible
    {

        int RequestProcMsg(ICtkMsg msg);

    }
}
