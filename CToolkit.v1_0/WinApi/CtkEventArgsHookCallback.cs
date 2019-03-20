using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_0.WinApi
{
    public class CtkEventArgsHookCallback : EventArgs
    {
        public int nCode;
        public IntPtr wParam;
        public IntPtr lParam;
    }
}
