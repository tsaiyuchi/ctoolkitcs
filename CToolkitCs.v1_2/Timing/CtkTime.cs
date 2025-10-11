using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CToolkitCs.v1_2.Timing
{
    /*[d20250923] 雖然有 CtkTimeUtil 可以進行各種轉換，但仍需要一個便利性的結構來操作*/



    public struct CtkTime : IComparable<CtkTime>
    {

        public const int YearDiffFromRocToAd = 1911;

        public CtkTime(DateTime dt) { this.DateTime = dt; }
        public CtkTime(long ticks) { this.DateTime = new DateTime(ticks); }


        public DateTime DateTime { get; set; }



        public int RocYear { get => this.DateTime.Year - YearDiffFromRocToAd; }
        public int CompareTo([AllowNull] CtkTime other) { return this.DateTime.CompareTo(other.DateTime); }

        public override bool Equals(object obj) { return base.Equals(obj); }
        public override int GetHashCode() { return base.GetHashCode(); }


        #region Static Method

        public static CtkTime AdToRocYear(int ad) { return ad - YearDiffFromRocToAd; }
        public static CtkTime FromRoc(int year, int month = 1, int day = 1, int hour = 0, int minute = 0, int second = 0)
        {
            return new DateTime(year + YearDiffFromRocToAd, month, day, hour, minute, second);
        }

        public static CtkTime RocToAdYear(int roc) { return roc + YearDiffFromRocToAd; }

        #endregion


        #region Operator

        public static implicit operator CtkTime(long d) { return new CtkTime(d); }
        public static implicit operator CtkTime(DateTime dt) { return new CtkTime(dt); }
        public static implicit operator DateTime(CtkTime dt) { return dt.DateTime; }

        public static bool operator !=(CtkTime a, CtkTime b) { return a.CompareTo(b) != 0; }
        public static bool operator <(CtkTime a, CtkTime b) { return a.CompareTo(b) < 0; }
        public static bool operator <=(CtkTime a, CtkTime b) { return a.CompareTo(b) <= 0; }
        public static bool operator ==(CtkTime a, CtkTime b) { return a.CompareTo(b) == 0; }
        public static bool operator >(CtkTime a, CtkTime b) { return a.CompareTo(b) > 0; }
        public static bool operator >=(CtkTime a, CtkTime b) { return a.CompareTo(b) >= 0; }

        #endregion


    }
}
