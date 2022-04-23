﻿using CToolkit.v1_1.Numeric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CToolkit.v1_1.Numeric
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
