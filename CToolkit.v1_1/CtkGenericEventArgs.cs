﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_1
{
    public class CtkGenericEventArgs<T> : EventArgs
    {
        public T Data;

        public static implicit operator CtkGenericEventArgs<T>(T data)
        {
            var ea = new CtkGenericEventArgs<T>();
            ea.Data = data;
            return ea;
        }

    }
}
