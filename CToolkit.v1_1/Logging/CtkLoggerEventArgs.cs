﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_1.Logging
{

    public class CtkLoggerEventArgs : EventArgs
    {
        public string Message;
        public Exception Exception;
        public CtkLoggerEnumLevel Level;



        public static implicit operator CtkLoggerEventArgs(string msg)
        {
            var ea = new CtkLoggerEventArgs();
            ea.Message = msg;
            ea.Level = CtkLoggerEnumLevel.Info;
            return ea;
        }
        public static implicit operator CtkLoggerEventArgs(Exception ex)
        {
            var ea = new CtkLoggerEventArgs();
            ea.Message = ex.Message;
            ea.Exception = ex;
            ea.Level = CtkLoggerEnumLevel.Warn;
            return ea;
        }
    }
}
