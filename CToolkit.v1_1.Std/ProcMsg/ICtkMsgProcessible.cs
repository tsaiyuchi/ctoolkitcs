﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CToolkit.v1_1.ProcMsg
{
    public interface ICtkMsgProcessible
    {

        ICtkMsg CtkProcCall(ICtkMsg msg);
        int CtkProcMsgRequest(ICtkMsg msg);
        ICtkMsg CtkProcMsg(ICtkMsg msg);
        int CtkProcWork(ICtkMsg msg);

    }
}
