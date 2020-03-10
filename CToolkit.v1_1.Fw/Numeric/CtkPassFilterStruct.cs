using CToolkit.v1_1.Numeric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_1.Numeric
{
    /// <summary>
    /// 使用Struct傳入是傳值, 修改是無法帶出來的, 但你可以回傳同一個結構後接住它
    /// </summary>
    public struct CtkPassFilterStruct
    {
        public int SampleRate;
        public CtkEnumPassFilterMode Mode;
        public int CutoffHigh;
        public int CutoffLow;
    }
}
