using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CToolkitCs.v1_2Core.WinApiNative
{

    [StructLayout(LayoutKind.Sequential)]
    public struct CtkStructSystemTime
    {
        public short wYear;
        public short wMonth;
        public short wDayOfWeek;
        public short wDay;
        public short wHour;
        public short wMinute;
        public short wSecond;
        public short wMilliseconds;
    }
}
