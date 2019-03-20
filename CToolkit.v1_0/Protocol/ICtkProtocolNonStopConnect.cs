using CToolkit.v1_0.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_0.Protocol
{
    public interface ICtkProtocolNonStopConnect : ICtkProtocolConnect
    {
        int IntervalTimeOfConnectCheck { get; set; }
        bool IsNonStopRunning { get; }
        void AbortNonStopConnect();

        void NonStopConnectAsyn();
    }
}
