using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_0.WinApiNative
{
    [Flags()]
    public enum CtkEnumKeyboardFlag : int
    {
        EXTENDEDKEY = 1,
        KEYUP = 2,
        UNICODE = 4,
        SCANCODE = 8
    }

}
