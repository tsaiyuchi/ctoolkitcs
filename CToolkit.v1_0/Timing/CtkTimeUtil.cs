using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CToolkit.v1_0.Timing
{
    public class CtkTimeUtil
    {
        //ToUniversalTime/ToLocalTime 會自動判別Kind = Local / Utc 來決定加減
        //若為Unspecified, 則可當兩者,
        // toLocal: +8 & Kink = Local
        // toUniversal: -8 & Kind = Utc



        //--- DateTime and Timestamp converter ---------

        //--- ROC ---------
        const int RocYearToYear = 1911;



        public static int QuarterOfYear(DateTime dt) { return (dt.Month - 1) / 3 + 1; }


        #region Week Operation
        //--- Week ---------


        /// <summary>
        /// 不超過當前日期的 dow(Day Of Week) (e.q.周二) 是哪天
        /// </summary>
        /// <param name="dow"></param>
        /// <returns></returns>
        public static DateTime GetLastDow(DayOfWeek dow) { return GetLastDow(dow, DateTime.Now); }

        public static DateTime GetLastDow(DayOfWeek dow, DateTime date)
        {

            var rs = date.AddDays((int)dow - (int)date.DayOfWeek);

            //如果超過當前日期, 就把它減回來
            if (rs > date)
                rs = rs.AddDays(-7);

            return rs;
        }

        public static DateTime GetThisDow(DayOfWeek dow) { return GetThisDow(dow, DateTime.Now); }

        public static DateTime GetThisDow(DayOfWeek dow, DateTime date) { return date.AddDays((int)dow - (int)date.DayOfWeek); }

        public static DateTime GetWeeklyEnd(DateTime date)
        {
            var last = GetThisDow(DayOfWeek.Saturday, date);
            return last;
        }

        public static DateTime GetWeeklyEndInSameYear(DateTime date)
        {
            var last = GetWeeklyEnd(date);
            if (last.Year > date.Year) last = new DateTime(date.Year, 12, 31);
            return last;
        }

        public static DateTime GetWeeklyStart(DateTime date)
        {
            var first = GetThisDow(DayOfWeek.Sunday, date);
            return first;
        }

        public static DateTime GetWeeklyStartInSameYear(DateTime date)
        {
            var first = GetWeeklyStart(date);
            if (first.Year < date.Year) first = new DateTime(date.Year, 1, 1);
            return first;
        }

        public static int GetWeekOfYear(DateTime date)
        {
            return CultureInfo
               .InvariantCulture
               .Calendar
               .GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }

        #endregion




        #region DateTime / String

        public static DateTime DateTimeParseExact(string s, string format = "yyyyMMdd") { return DateTime.ParseExact(s, format, CultureInfo.InvariantCulture); }
        public static DateTime DateTimeParseExact(string s, DateTime defaultDt, string format = "yyyyMMdd")
        {
            var dt = defaultDt;
            DateTimeTryParseExact(s, out dt);
            return dt;
        }

        public static bool DateTimeTryParseExact(string s, out DateTime result, string format = "yyyyMMdd") { return DateTime.TryParseExact(s, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result); }


        public static DateTime FromDTime(string s) { return FromYyyyMmDdHhIiSs(s); }
        public static bool FromDTimeTry(string s, out DateTime dt) { return FromYyyyMmDdHhIiSsTry(s, out dt); }
        public static DateTime FromYyyy(string s) { return DateTimeParseExact(s, "yyyy"); }
        public static DateTime FromYyyyMm(string s) { return DateTimeParseExact(s, "yyyyMM"); }
        public static DateTime FromYyyyMmDd(string s) { return DateTimeParseExact(s, "yyyyMMdd"); }
        public static DateTime FromYyyyMmDdHh(string s) { return DateTimeParseExact(s, "yyyyMMddHH"); }
        public static DateTime FromYyyyMmDdHhIi(string s) { return DateTimeParseExact(s, "yyyyMMddHHmm"); }
        public static DateTime FromYyyyMmDdHhIiSs(string s) { return DateTimeParseExact(s, "yyyyMMddHHmmss"); }
        public static bool FromYyyyMmDdHhIiSsTry(string s, out DateTime dt) { return DateTimeTryParseExact(s, out dt, "yyyyMMddHHmmss"); }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="yyyyqq"></param>
        /// <returns>該季第一天</returns>
        public static DateTime FromYyyyQq(string yyyyqq)
        {
            var yyyy = Convert.ToInt32(yyyyqq.Substring(0, 4));
            var qq = Convert.ToInt32(yyyyqq.Substring(4));

            var date = new DateTime(yyyy, 1, 1);
            date = date.AddMonths((qq - 1) * 3);

            var realYyyyQq = ToYyyyQq(date);
            if (yyyyqq != realYyyyQq) throw new InvalidOperationException();

            return date;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="yyyyww"></param>
        /// <returns>該周的某天</returns>
        public static DateTime FromYyyyWw(string yyyyww)
        {
            var yyyy = Convert.ToInt32(yyyyww.Substring(0, 4));
            var ww = Convert.ToInt32(yyyyww.Substring(4));

            var date = new DateTime(yyyy, 1, 1);
            date = date.AddDays(7 * ww - 7);

            var realYyyyww = ToYyyyWw(date);
            if (yyyyww != realYyyyww) throw new InvalidOperationException();

            return date;
        }


        public static string ToDTime(DateTime dt) { return ToYyyyMmDdHhIiSs(dt); }

        public static string ToYyyy(DateTime dt) { return dt.ToString("yyyy"); }
        public static string ToYyyyMm(DateTime dt) { return dt.ToString("yyyyMM"); }
        public static string ToYyyyMmDd(DateTime dt) { return dt.ToString("yyyyMMdd"); }
        public static string ToYyyyMmDdHh(DateTime dt) { return dt.ToString("yyyyMMddHH"); }
        public static string ToYyyyMmDdHhIi(DateTime dt) { return dt.ToString("yyyyMMddHHmm"); }
        public static string ToYyyyMmDdHhIiSs(DateTime dt) { return dt.ToString("yyyyMMddHHmmss"); }
        public static string ToYyyyQq(DateTime dt)
        {
            var qq = QuarterOfYear(dt);
            return string.Format("{0}{1:00}", dt.ToString("yyyy"), qq);
        }
        public static string ToYyyyWw(DateTime dt)
        {
            var weekOfYear = CtkTimeUtil.GetWeekOfYear(dt);
            return string.Format("{0}{1:00}", dt.ToString("yyyy"), weekOfYear);
        }



        #endregion



        #region Sign DateTime / String

        public static DateTime FromSYyyy(string yyyy)
        {
            if (!yyyy.StartsWith("y")) throw new ArgumentException("錯誤的Sign");
            yyyy = yyyy.Substring(1);
            return FromYyyy(yyyy);
        }

        public static DateTime FromSYyyyMm(string yyyymm)
        {
            if (!yyyymm.StartsWith("m")) throw new ArgumentException("錯誤的Sign");
            yyyymm = yyyymm.Substring(1);
            return FromYyyyMm(yyyymm);
        }

        public static DateTime FromSYyyyMmDd(string yyyymmdd)
        {
            if (!yyyymmdd.StartsWith("d")) throw new ArgumentException("錯誤的Sign");
            yyyymmdd = yyyymmdd.Substring(1);
            return FromYyyyMmDd(yyyymmdd);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="yyyyqq"></param>
        /// <returns>該季第一天</returns>
        public static DateTime FromSYyyyQq(string yyyyqq)
        {
            if (!yyyyqq.StartsWith("q")) throw new ArgumentException("錯誤的Sign");
            yyyyqq = yyyyqq.Substring(1);
            return FromYyyyQq(yyyyqq);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="yyyyww"></param>
        /// <returns>該周的某天</returns>
        public static DateTime FromSYyyyWw(string yyyyww)
        {
            if (!yyyyww.StartsWith("w")) throw new ArgumentException("錯誤的Sign");
            yyyyww = yyyyww.Substring(1);
            return FromYyyyWw(yyyyww);
        }

        public static string ToSYyyy(DateTime dt) { return "y" + dt.ToString("yyyy"); }
        public static string ToSYyyyMm(DateTime dt) { return "m" + dt.ToString("yyyyMM"); }
        public static string ToSYyyyMmDd(DateTime dt) { return "d" + dt.ToString("yyyyMMdd"); }
        public static string ToSYyyyQq(DateTime dt)
        {
            var qq = QuarterOfYear(dt);
            return string.Format("q{0}{1:00}", dt.ToString("yyyy"), qq);
        }
        public static string ToSYyyyWw(DateTime dt)
        {
            var weekOfYear = CtkTimeUtil.GetWeekOfYear(dt);
            return string.Format("w{0}{1:00}", dt.ToString("yyyy"), weekOfYear);
        }
        #endregion


        #region Linux Timestamp

        public static DateTime ToDateTimeFromMilliTimestamp(double timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(timestamp);
        }

        public static DateTime ToDateTimeFromTimestamp(double timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(timestamp);
        }

        public static DateTime ToLocalDateTimeFromTimestamp(double timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(timestamp).ToLocalTime();
        }


        public static Int64 ToMilliTimestamp()
        {
            return ToMilliTimestamp(DateTime.Now);
        }
        public static Int64 ToMilliTimestamp(DateTime dt)
        {
            return (Int64)(dt - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
        }

        public static double ToTimestamp()
        {
            return ToTimestamp(DateTime.Now);
        }
        public static double ToTimestamp(DateTime dt)
        {
            return (dt - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }

        public static Int64 ToUtcMilliTimestamp(DateTime dt)
        {
            return (Int64)(dt.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
        }

        public static double ToUtcTimestamp()
        {
            return ToUtcTimestamp(DateTime.Now);
        }
        public static double ToUtcTimestamp(DateTime dt)
        {
            return (dt.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }

        #endregion

        #region ROC DateTime

        public static DateTime ToDateTimeFromRoc(DateTime dt) { return dt.AddYears(RocYearToYear); }
        public static DateTime ToDateTimeFromRocYyyMmDd(string dt, char spliter)
        {
            var nums = dt.Split(spliter);
            var yyy = Convert.ToInt32(nums[0]);
            var mm = Convert.ToInt32(nums[1]);
            var dd = Convert.ToInt32(nums[2]);
            var datetime = new DateTime(yyy, mm, dd);
            return ToDateTimeFromRoc(datetime);
        }

        public static DateTime ToRocDateTime(DateTime dt) { return dt.AddYears(-RocYearToYear); }

        public static int ToRocYear(int year) { return year - RocYearToYear; }

        public static int ToYearFromRoc(int year) { return year + RocYearToYear; }
        #endregion


        #region Transfer Date Time

        /// <summary>
        /// 取得下一個的日
        /// </summary>
        public static DateTime GetNextDay(DateTime dt, int day = 0, bool isIncludeToday = true)
        {
            var diff = day - dt.Day;//上一個期望時間, 若為正, 就代表己越過零點
            var mydt = dt;
            if (isIncludeToday && diff < 0)
                mydt.AddMonths(1);
            else if (!isIncludeToday && diff <= 0)
                mydt.AddMonths(1);

            return new DateTime(mydt.Year, mydt.Month, day);
        }

        /// <summary>
        /// 取得過往的指定時分秒
        /// </summary>
        public static DateTime GetNextTime(DateTime dt, int hour = 0, int minute = 0, int second = 0, bool isIncludeToday = true)
        {
            var diff = hour - dt.Hour;//上一個期望時間, 若為正, 就代表己越過零點
            var mydt = dt;
            if (isIncludeToday && diff < 0)
                mydt = dt.AddHours(24);
            else if (!isIncludeToday && diff <= 0)
                mydt = dt.AddHours(24);
            return new DateTime(mydt.Year, mydt.Month, mydt.Day, hour, minute, second);
        }

        /// <summary>
        /// 取得己過往的日
        /// </summary>
        public static DateTime GetPrevDay(DateTime dt, int day = 0, bool isIncludeToday = true)
        {
            var diff = day - dt.Day;//上一個期望時間, 若為正, 就代表己越過零點
            var mydt = dt;
            if (isIncludeToday && diff > 0)
                mydt.AddMonths(-1);
            else if (!isIncludeToday && diff >= 0)
                mydt.AddMonths(-1);

            return new DateTime(mydt.Year, mydt.Month, day);
        }

        /// <summary>
        /// 取得過往的指定時分秒
        /// </summary>
        public static DateTime GetPrevTime(DateTime dt, int hour = 0, int minute = 0, int second = 0, bool isIncludeToday = true)
        {
            var diff = hour - dt.Hour;//上一個期望時間, 若為正, 就代表己越過零點
            var mydt = dt;
            if (isIncludeToday && diff > 0)
                mydt = dt.AddHours(-24);
            else if (!isIncludeToday && diff >= 0)
                mydt = dt.AddHours(-24);
            return new DateTime(mydt.Year, mydt.Month, mydt.Day, hour, minute, second);
        }
        #endregion

        #region Compare

        public static int CompareDTime(DateTime dt1, DateTime dt2) { return string.Compare(ToDTime(dt1), ToDTime(dt2)); }
        public static int CompareDTime(DateTime dt1, string dt2) { return string.Compare(ToDTime(dt1), dt2); }
        public static int CompareDTime(string dt1, DateTime dt2) { return string.Compare(dt1, ToDTime(dt2)); }

        public static int CompareYyyyMm(DateTime dt1, DateTime dt2) { return string.Compare(ToYyyyMm(dt1), ToYyyyMm(dt2)); }
        public static int CompareYyyy(DateTime dt1, DateTime dt2) { return string.Compare(ToYyyy(dt1), ToYyyy(dt2)); }
        public static int CompareYyyy(DateTime dt1, string dt2) { return string.Compare(ToYyyy(dt1), dt2); }
        public static int CompareYyyy(string dt1, DateTime dt2) { return string.Compare(dt1, ToYyyy(dt2)); }

        public static int CompareYyyyMmDd(DateTime dt1, DateTime dt2) { return string.Compare(ToYyyyMmDd(dt1), ToYyyyMmDd(dt2)); }
        public static int CompareYyyyMmDd(DateTime dt1, string dt2) { return string.Compare(ToYyyyMmDd(dt1), dt2); }
        public static int CompareYyyyMmDd(string dt1, DateTime dt2) { return string.Compare(dt1, ToYyyyMmDd(dt2)); }

        public static int CompareYyyyQq(DateTime dt1, DateTime dt2) { return string.Compare(ToYyyyQq(dt1), ToYyyyQq(dt2)); }

        public static int CompareYyyyWw(DateTime dt1, DateTime dt2) { return string.Compare(ToYyyyWw(dt1), ToYyyyWw(dt2)); }
        #endregion


    }
}
