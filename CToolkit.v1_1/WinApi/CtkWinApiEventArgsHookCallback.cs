﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_1.WinApi
{
    public class CtkWinApiEventArgsHookCallback : EventArgs
    {
        public int nCode;
        public IntPtr wParam;
        public IntPtr lParam;
    }
}
