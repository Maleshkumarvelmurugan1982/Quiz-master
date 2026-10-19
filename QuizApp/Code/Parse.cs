using System.Globalization;

namespace QuizApp.Code
{
    /// <summary>Safe number parsing for form fields (accepts "0.25" regardless of server culture).</summary>
    public static class Parse
    {
        public static int Int(string value, int fallback)
        {
            int v;
            return int.TryParse((value ?? "").Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out v) ? v : fallback;
        }

        public static decimal Decimal(string value, decimal fallback)
        {
            decimal v;
            value = (value ?? "").Trim();
            if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out v)) return v;
            if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out v)) return v;
            return fallback;
        }
    }
}
