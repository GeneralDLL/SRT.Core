// Ignore Spelling: SRT Substring

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.SRTExtensions
{
    public static class StringExtensions
    {
        public static string SRT_String_NumberEnglishToPersian(this string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return string.Empty;

            string[] persian = new string[10] { "۰", "۱", "۲", "۳", "۴", "۵", "۶", "۷", "۸", "۹" };

            for (int j = 0; j < persian.Length; j++)
                data = data.Replace(j.ToString(), persian[j]);

            return data;
        }
        public static string SRT_String_NumberPersianToEnglish(this string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return string.Empty;

            string[] persian = new string[10] { "۰", "۱", "۲", "۳", "۴", "۵", "۶", "۷", "۸", "۹" };

            for (int j = 0; j < persian.Length; j++)
                data = data.Replace(persian[j], j.ToString());

            return data;
        }

        public static string SRT_String_ArabicToPersian(this string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return string.Empty;

            // تعریف مپ برای تبدیل حروف عربی به حروف فارسی
            var arabicToPersianMap = new Dictionary<char, char>
            {
                {'ي', 'ی'},
                {'ك', 'ک'},
                {'ة', 'ه'},
                {'ء', 'ئ'},
                {'ٱ', 'ا'},
                {'أ', 'ا'},
                {'إ', 'ا'},
                {'ؤ', 'و'},
                {'ئ', 'ی'},
                {'ى', 'ی'},
                {'آ', 'آ'}
            };

            foreach (var item in arabicToPersianMap)
            { data = data.Replace(item.Key, item.Value); }

            return data;
        }
        public static string SRT_String_CheckReady(this string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return string.Empty;

            return data.SRT_String_ArabicToPersian().SRT_String_NumberPersianToEnglish();
        }

        public static bool SRT_String_IsNullOrEmpty([NotNullWhen(false)] this string? data)
        { return string.IsNullOrWhiteSpace(data); }

        public static string SRT_String_Substring(this string? data, int startIndex, int length)
        {
            if (string.IsNullOrWhiteSpace(data))
                return string.Empty;
            if (length > data.Length + startIndex)
                return data;

            return data.Substring(startIndex, length);
        }
        public static string SRT_String_Substring([NotNullWhen(false)] this string? data, int length) => data.SRT_String_Substring(0, length);
        public static string SRT_String_Substring_Between([NotNullWhen(false)] this string? data, string from, string to)
        {
            if (string.IsNullOrWhiteSpace(data))
                return string.Empty;

            int startIndex = string.IsNullOrWhiteSpace(data) ? -1 : data.IndexOf(from) + from.Length;
            int endIndex = string.IsNullOrWhiteSpace(to) ? -1 : data.IndexOf(to, startIndex);

            if (startIndex < 0)
                startIndex = 0;
            if (endIndex < 0)
                return string.Empty;

            return data.Substring(startIndex, endIndex - startIndex);
        }

        public static decimal? SRT_String_GetNumbersFromString(this string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return null;

            string b = string.Empty;

            try
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (char.IsDigit(data[i]) || data[i] == 46)
                        b += data[i];
                }

                if (data.IndexOf("-") == 0 || data.IndexOf("(") == 0)
                    return -1 * decimal.Parse(b);

                return decimal.Parse(b);
            }
            catch { return null; }
        }

        public static IEnumerable<string> SRT_String_Split(this string? data, string splitValue, StringSplitOptions splitOptions = StringSplitOptions.None)
        {
            if (string.IsNullOrWhiteSpace(data))
                return new List<string>();

            return data.Split(new string[] { splitValue }, splitOptions).ToList();
        }
        public static bool SRT_String_IsNumber(this string? data)
        { return decimal.TryParse(data, out decimal a); }
        public static bool? ToBool(this string? data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                    return false;

                if (!decimal.TryParse(data, out decimal a))
                {
                    if (data.ToLower().Replace(" ", "") == "true")
                        return true;

                    return false;
                }
                if (a == 0)
                    return false;

                return true;
            }
            catch
            {
                return null;
            }
        }

        public static System.Drawing.Color? SRT_String_ToColor(this string? data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                    return null;

                #region ARGB
                if (data.Count(q => q == ',') == 2)
                {
                    var d = data.Split(',');
                    return System.Drawing.Color.FromArgb(int.Parse(d[0]), int.Parse(d[1]), int.Parse(d[2]));
                }
                if (data.Count(q => q == ',') == 3)
                {
                    var d = data.Split(',');
                    return System.Drawing.Color.FromArgb(int.Parse(d[0]), int.Parse(d[1]), int.Parse(d[2]), int.Parse(d[3]));
                }
                #endregion

                #region HTML
                if (data.IndexOf("#") < 0)
                    data = "#" + data;
                //TODO: Convert # To Color
                throw new NotImplementedException();
                #endregion
            }
            catch
            { return null; }
        }

    }
}
