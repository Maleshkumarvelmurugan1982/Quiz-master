using System;
using System.Globalization;
using System.Web;

namespace QuizApp.Code
{
    /// <summary>Formatting helpers used inside .aspx data-binding expressions.</summary>
    public static class Fmt
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        /// <summary>HTML-encodes any value for safe output in markup: &lt;%# Fmt.H(Eval("Title")) %&gt;</summary>
        public static string H(object value)
        {
            return HttpUtility.HtmlEncode(value == null || value == DBNull.Value ? "" : Convert.ToString(value));
        }

        public static string Date(object value)
        {
            if (value == null || value == DBNull.Value) return "—";
            var d = Convert.ToDateTime(value);
            return d == DateTime.MinValue ? "—" : d.ToString("dd MMM yyyy", Inv);
        }

        public static string DateAndTime(object value)
        {
            if (value == null || value == DBNull.Value) return "—";
            var d = Convert.ToDateTime(value);
            return d == System.DateTime.MinValue ? "—" : d.ToString("dd MMM yyyy, hh:mm tt", Inv);
        }

        /// <summary>Seconds -> "mm:ss".</summary>
        public static string Time(object seconds)
        {
            var s = seconds == null || seconds == DBNull.Value ? 0 : Convert.ToInt32(seconds);
            if (s < 0) s = 0;
            return (s / 60).ToString("00", Inv) + ":" + (s % 60).ToString("00", Inv);
        }

        /// <summary>Decimal without trailing zeros: 66.50 -> "66.5", 2.00 -> "2".</summary>
        public static string Num(object value)
        {
            if (value == null || value == DBNull.Value) return "0";
            return Convert.ToDecimal(value).ToString("0.##", Inv);
        }

        public static string Round(object value)
        {
            if (value == null || value == DBNull.Value) return "0";
            return Math.Round(Convert.ToDecimal(value), MidpointRounding.AwayFromZero).ToString("0", Inv);
        }

        public static string DiffBadge(object difficulty)
        {
            var d = Convert.ToString(difficulty);
            return d == "Easy" ? "badge-green" : d == "Hard" ? "badge-red" : "badge-amber";
        }

        public static string StatusBadge(object status)
        {
            return Convert.ToString(status) == "Passed" ? "badge-green" : "badge-red";
        }

        public static string ActiveBadge(object isActive)
        {
            return Convert.ToBoolean(isActive) ? "badge-green" : "badge-red";
        }

        public static string Truncate(object value, int max)
        {
            var s = Convert.ToString(value) ?? "";
            return s.Length <= max ? s : s.Substring(0, max) + "...";
        }

        /// <summary>CSS height/width percentage (invariant culture, clamped 4-100).</summary>
        public static string BarHeight(object pct)
        {
            var p = pct == null || pct == DBNull.Value ? 0m : Convert.ToDecimal(pct);
            if (p < 4) p = 4;
            if (p > 100) p = 100;
            return p.ToString("0.##", Inv);
        }

        public static string Initial(string name)
        {
            return string.IsNullOrEmpty(name) ? "?" : name.Substring(0, 1).ToUpperInvariant();
        }

        public static string Js(string value)
        {
            return HttpUtility.JavaScriptStringEncode(value ?? "");
        }

        public static string FirstWord(object value)
        {
            var s = (Convert.ToString(value) ?? "").Trim();
            var i = s.IndexOf(' ');
            return i > 0 ? s.Substring(0, i) : s;
        }
    }
}
