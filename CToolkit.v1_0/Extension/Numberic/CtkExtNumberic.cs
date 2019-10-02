using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_0.Extension.Numberic
{
    public static class CtkExtNumberic
    {
        public static double ValueOrZero(this Nullable<double> val)
        {
            if (val.HasValue)
                return val.Value;

            return 0.0;
        }

        public static double CtkVal(this Nullable<double> val, double defVal)
        {
            if (val.HasValue)
                return val.Value;

            return defVal;
        }

        public static decimal ValueOrZero(this Nullable<decimal> val)
        {
            if (val.HasValue)
                return val.Value;
            return 0;
        }

        public static double DoubleValueOrZero(this Nullable<decimal> val)
        {
            if (val.HasValue)
                return (double)val.Value;
            return 0;
        }

        public static decimal CtkParseVal(this Nullable<decimal> val)
        {
            if (val.HasValue)
                return val.Value;

            throw new ArgumentException("值為null");
        }

    }
}
