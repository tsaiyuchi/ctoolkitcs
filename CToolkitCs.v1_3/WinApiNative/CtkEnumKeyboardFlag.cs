using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkitCs.v1_3.WinApiNative
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
