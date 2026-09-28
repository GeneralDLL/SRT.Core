using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace SRT.Core
{
    public sealed class DateTimePersian :
        IEquatable<DateTimePersian>,
        IComparable<DateTimePersian>,
        IComparable
    {
        #region Season Enum
        public enum SeasonEnum
        {
            بهار = 1,
            تابستان = 2,
            پاییز = 3,
            زمستان = 4
        }
        public enum MonthEnum
        {
            فروردین = 1,
            اردیبهشت = 2,
            خرداد = 3,
            تیر = 4,
            مرداد = 5,
            شهریور = 6,
            مهر = 7,
            آبان = 8,
            آذر = 9,
            دی = 10,
            بهمن = 11,
            اسفند = 12,
        }
        #endregion

        private static readonly PersianCalendar _persianCalendar = new();
        private static readonly CultureInfo _persianCulture;

        static DateTimePersian()
        {
            _persianCulture = new CultureInfo("fa-IR");
            _persianCulture.DateTimeFormat.Calendar = _persianCalendar;
        }

        public DateTime DateMilady { get; private set; }

        #region Constructors

        public DateTimePersian()
            : this(DateTime.Now) { }

        public DateTimePersian(int year, int month, int day, int hour = 0, int minute = 0, int second = 0, int millisecond = 0)
            : this(_persianCalendar.ToDateTime(year, month, day, hour, minute, second, millisecond)) { }

        public DateTimePersian(DateTime date)
        {
            DateMilady = date;
        }

        #endregion

        #region Properties (Mutable)

        public int Year
        {
            get => _persianCalendar.GetYear(DateMilady);
            set => AddYears(value);
        }

        public MonthEnum Month
        {
            get => (MonthEnum)_persianCalendar.GetMonth(DateMilady);
            set => AddMonths((int)value);
        }

        public int Day
        {
            get => _persianCalendar.GetDayOfMonth(DateMilady);
            set => AddDays(value);
        }

        public int Hour
        {
            get => _persianCalendar.GetHour(DateMilady);
            set => AddHours(value);
        }

        public int Minute
        {
            get => _persianCalendar.GetMinute(DateMilady);
            set => AddMinutes(value);
        }

        public int Seconds
        {
            get => _persianCalendar.GetSecond(DateMilady);
            set => AddSeconds(value);
        }

        /// <summary>
        /// نام فارسی ماه (مثلا: فروردین، اردیبهشت، شهریور ...)
        /// </summary>
        public string MonthName => DateMilady.ToString("MMMM", _persianCulture);

        /// <summary>
        /// نام فصل فارسی بر اساس ماه شمسی
        /// </summary>
        public SeasonEnum Season
        {
            get
            {
                return (int)Month switch
                {
                    1 or 2 or 3 => SeasonEnum.بهار,
                    4 or 5 or 6 => SeasonEnum.تابستان,
                    7 or 8 or 9 => SeasonEnum.پاییز,
                    10 or 11 or 12 => SeasonEnum.زمستان,
                    _ => throw new InvalidOperationException("ماه نامعتبر است")
                };
            }
        }

        #endregion

        #region Add Methods (Mutable)

        public DateTimePersian AddYears(int value)
        {
            DateMilady = DateMilady.AddYears(value);
            return this;
        }

        public DateTimePersian AddMonths(int value)
        {
            DateMilady = DateMilady.AddMonths(value);
            return this;
        }

        public DateTimePersian AddDays(int value)
        {
            DateMilady = DateMilady.AddDays(value);
            return this;
        }

        public DateTimePersian AddHours(int value)
        {
            DateMilady = DateMilady.AddHours(value);
            return this;
        }

        public DateTimePersian AddMinutes(int value)
        {
            DateMilady = DateMilady.AddMinutes(value);
            return this;
        }

        public DateTimePersian AddSeconds(int value)
        {
            DateMilady = DateMilady.AddSeconds(value);
            return this;
        }

        #endregion

        #region Add Methods (Immutable)

        public DateTimePersian AddYearsImmutable(int value) => new(DateMilady.AddYears(value));
        public DateTimePersian AddMonthsImmutable(int value) => new(DateMilady.AddMonths(value));
        public DateTimePersian AddDaysImmutable(int value) => new(DateMilady.AddDays(value));
        public DateTimePersian AddHoursImmutable(int value) => new(DateMilady.AddHours(value));
        public DateTimePersian AddMinutesImmutable(int value) => new(DateMilady.AddMinutes(value));
        public DateTimePersian AddSecondsImmutable(int value) => new(DateMilady.AddSeconds(value));

        #endregion

        #region Conversion

        public int GetCurrentMonthDays() => _persianCalendar.GetDaysInMonth(Year, (int)Month);

        #endregion

        #region Operator Overloading

        public static bool operator ==(DateTimePersian? a, DateTimePersian? b) => Equals(a, b);
        public static bool operator !=(DateTimePersian? a, DateTimePersian? b) => !Equals(a, b);

        public static bool operator <(DateTimePersian a, DateTimePersian b) => a.DateMilady < b.DateMilady;
        public static bool operator >(DateTimePersian a, DateTimePersian b) => a.DateMilady > b.DateMilady;
        public static bool operator <=(DateTimePersian a, DateTimePersian b) => a.DateMilady <= b.DateMilady;
        public static bool operator >=(DateTimePersian a, DateTimePersian b) => a.DateMilady >= b.DateMilady;

        #endregion

        #region Equality & Comparison

        public bool Equals(DateTimePersian? other) => other is not null && DateMilady == other.DateMilady;
        public override bool Equals(object? obj) => obj is DateTimePersian other && Equals(other);
        public override int GetHashCode() => DateMilady.GetHashCode();

        public int CompareTo(DateTimePersian? other) =>
            other is null ? 1 : DateMilady.CompareTo(other.DateMilady);

        int IComparable.CompareTo(object? obj)
        {
            if (obj is null) return 1;
            if (obj is DateTimePersian other) return CompareTo(other);
            throw new ArgumentException("Object is not a DateTimePersian");
        }

        #endregion

        #region ToString Methods

        public override string ToString() => ToString(includeTime: true);

        public string ToString([StringSyntax("DateTimeFormat")] string? format)
        {
            format = string.IsNullOrWhiteSpace(format) ? "yyyy/MM/dd HH:mm:ss" : format;
            return DateMilady.ToString(format, _persianCulture);
        }

        /// <summary>
        /// include time: yyyy/MM/dd HH:mm:ss
        /// without time: yyyy/MM/dd
        /// </summary>
        /// <param name="includeTime"></param>
        /// <returns></returns>
        public string ToString(bool includeTime)
            => ToString(includeTime ? null : "yyyy/MM/dd");

        #endregion
    }
}