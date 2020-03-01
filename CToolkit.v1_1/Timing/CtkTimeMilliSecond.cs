using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_1.Timing
{
    public struct CtkTimeMilliSecond : IComparable<CtkTimeMilliSecond>
    {
        public long TotalMilliSecond;
        public CtkTimeMilliSecond(DateTime dt)
        {
            var span = dt - new DateTime();
            this.TotalMilliSecond = (long)span.TotalMilliseconds;
        }

        public CtkTimeMilliSecond(long total = 0) { this.TotalMilliSecond = total; }

        public DateTime DateTime { get { return new DateTime(TotalMilliSecond * TimeSpan.TicksPerMillisecond); } }

        public static implicit operator CtkTimeMilliSecond(long d) { return new CtkTimeMilliSecond(d); }

        public static implicit operator CtkTimeMilliSecond(DateTime dt) { return new CtkTimeMilliSecond(dt); }

        public int CompareTo(CtkTimeMilliSecond other) { return this.TotalMilliSecond.CompareTo(other.TotalMilliSecond); }



        public static bool operator !=(CtkTimeMilliSecond a, CtkTimeMilliSecond b) { return a.CompareTo(b) != 0; }

        public static bool operator <(CtkTimeMilliSecond a, CtkTimeMilliSecond b) { return a.CompareTo(b) < 0; }

        public static bool operator <=(CtkTimeMilliSecond a, CtkTimeMilliSecond b) { return a.CompareTo(b) <= 0; }

        public static bool operator ==(CtkTimeMilliSecond a, CtkTimeMilliSecond b) { return a.CompareTo(b) == 0; }

        public static bool operator >(CtkTimeMilliSecond a, CtkTimeMilliSecond b) { return a.CompareTo(b) > 0; }

        public static bool operator >=(CtkTimeMilliSecond a, CtkTimeMilliSecond b) { return a.CompareTo(b) >= 0; }
    }
}
