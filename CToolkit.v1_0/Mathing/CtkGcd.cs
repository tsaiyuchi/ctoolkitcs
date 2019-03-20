using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_0.Mathing
{
    public class CtkGcd
    {
        public static int GCD(int a, int b)
        {
            while (b > 1)
            {
                int mod = a % b;
                a = b;
                b = mod;
            }
            if (b == 0) { return a; }
            return b;

        }
    }
}
