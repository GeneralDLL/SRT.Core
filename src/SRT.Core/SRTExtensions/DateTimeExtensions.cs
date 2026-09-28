// Ignore Spelling: SRT

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.SRTExtensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime">null => DateTime.Now</param>
        /// <returns></returns>
        public static DateTimePersian SRT_Persian_Convert(this DateTime? dateTime)
        { return new DateTimePersian(dateTime ?? DateTime.Now); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="format">null => "yyyy/MM/dd HH:mm:ss"</param>
        /// <returns></returns>
        public static string SRT_Persian_ToString(this DateTime? dateTime, [StringSyntax("DateTimeFormat")] string? format = null)
        {
            var datePersian = dateTime.SRT_Persian_Convert();

            return datePersian.ToString(format);
        }

        public static (string name, DateTimePersian.MonthEnum mounthPersian, int totallDays) SRT_Persian_GetMonthData(this DateTime? dateTime)
        {
            var datePersian = dateTime.SRT_Persian_Convert();

            return (datePersian.MonthName, datePersian.Month, datePersian.GetCurrentMonthDays());
        }
    }
}
