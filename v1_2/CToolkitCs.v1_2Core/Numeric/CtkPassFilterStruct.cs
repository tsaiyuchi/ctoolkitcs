using CToolkitCs.v1_2Core.Numeric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CToolkitCs.v1_2Core.Numeric
{
    /// <summary>
    /// 使用Struct傳入是傳值, 修改是無法帶出來的, 但你可以回傳同一個結構後接住它
    /// </summary>
    public struct CtkPassFilterStruct
    {

        [XmlAttribute] public int SampleRate;
        [XmlAttribute] public CtkEnumPassFilterMode Mode;
        [XmlAttribute] public int CutoffHigh;
        [XmlAttribute] public int CutoffLow;
    }
}
