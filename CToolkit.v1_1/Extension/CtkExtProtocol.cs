using CToolkit.v1_1.Protocol;
using CToolkit.v1_1.Wcf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_1.Extension
{
    public static class CtkExtProtocol
    {

        public static CtkProtocolTrxMessage FromWcfMessage(CtkWcfMessage msg) { return new CtkProtocolTrxMessage() { TrxMessage = msg }; }

    }
}
