using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_1.Timing
{
    /// <summary>
    /// 此類別其實等同於 DateTime
    /// 因此可以直接使用DateTime取代
    /// 這邊僅留存提示
    /// </summary>
    public struct CtkTimeTick : IComparable<CtkTimeTick>
    {
        public long TotalTicks;
        public CtkTimeTick(DateTime dt)
        {
            var span = dt - new DateTime();
            this.TotalTicks = (long)span.Ticks;
        }

        public CtkTimeTick(long total = 0) { this.TotalTicks = total; }

        public DateTime DateTime { get { return new DateTime(this.TotalTicks * TimeSpan.TicksPerMillisecond); } }

        public static implicit operator CtkTimeTick(long d) { return new CtkTimeTick(d); }

        public static implicit operator CtkTimeTick(DateTime dt) { return new CtkTimeTick(dt); }

        public int CompareTo(CtkTimeTick other) { return this.TotalTicks.CompareTo(other.TotalTicks); }



        public static bool operator !=(CtkTimeTick a, CtkTimeTick b) { return a.CompareTo(b) != 0; }

        public static bool operator <(CtkTimeTick a, CtkTimeTick b) { return a.CompareTo(b) < 0; }

        public static bool operator <=(CtkTimeTick a, CtkTimeTick b) { return a.CompareTo(b) <= 0; }

        public static bool operator ==(CtkTimeTick a, CtkTimeTick b) { return a.CompareTo(b) == 0; }

        public static bool operator >(CtkTimeTick a, CtkTimeTick b) { return a.CompareTo(b) > 0; }

        public static bool operator >=(CtkTimeTick a, CtkTimeTick b) { return a.CompareTo(b) >= 0; }
    }
}
