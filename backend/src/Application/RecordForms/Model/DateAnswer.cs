using System.Globalization;
using System.Text.RegularExpressions;
using UKPS.Api.Application.RecordForms.Rules;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Model;

/// <summary>
/// The answer of a date question. A <see cref="FormControl.PrecisionDate"/> takes <c>yyyy-Qn</c>
/// for an estimated quarter, <c>yyyy-MM</c> for an estimated month and <c>yyyy-MM-dd</c> for an
/// actual date; an <see cref="FormControl.ActualMonth"/> takes <c>yyyy-MM</c> only. Quarters and
/// months are stored as their first day. An actual date is today or earlier, an actual month this
/// month or earlier, and an estimate is for a quarter or month that has not ended.
/// </summary>
/// <remarks>Mirrored by <c>frontend/src/lib/dynamic-form/dates.ts</c>; <c>date-cases.json</c> keeps the two in step.</remarks>
internal static partial class DateAnswer
{
    private static readonly TimeZoneInfo _ukTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
        "Europe/London"
    );

    /// <summary>Today's date in the UK, which dates are checked against.</summary>
    public static DateOnly UkToday(DateTime utcNow) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utcNow, _ukTimeZone));

    public static string Format(DatePrecision precision, DateOnly value) =>
        precision switch
        {
            DatePrecision.EstimatedQuarter => string.Create(
                CultureInfo.InvariantCulture,
                $"{value.Year:D4}-Q{((value.Month - 1) / 3) + 1}"
            ),
            DatePrecision.EstimatedMonth or DatePrecision.ActualMonth => value.ToString(
                "yyyy-MM",
                CultureInfo.InvariantCulture
            ),
            DatePrecision.ActualDate => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            _ => throw new ArgumentOutOfRangeException(nameof(precision), precision, null),
        };

    /// <summary>
    /// Parses the answer, or returns false when it is not a real date in one of the control's
    /// formats.
    /// </summary>
    public static bool TryParse(
        FormControl control,
        string answer,
        out DatePrecision precision,
        out DateOnly value
    )
    {
        precision = default;
        value = default;
        Match match = Pattern().Match(answer);
        bool isMonth = match.Groups["month"].Success && !match.Groups["day"].Success;
        if (!match.Success || (control == FormControl.ActualMonth && !isMonth))
        {
            return false;
        }

        int year = int.Parse(match.Groups["year"].Value, CultureInfo.InvariantCulture);
        if (match.Groups["quarter"].Success)
        {
            precision = DatePrecision.EstimatedQuarter;
            int quarter = int.Parse(match.Groups["quarter"].Value, CultureInfo.InvariantCulture);
            value = new DateOnly(year, ((quarter - 1) * 3) + 1, 1);
            return true;
        }

        int month = int.Parse(match.Groups["month"].Value, CultureInfo.InvariantCulture);
        if (isMonth)
        {
            precision =
                control == FormControl.ActualMonth
                    ? DatePrecision.ActualMonth
                    : DatePrecision.EstimatedMonth;
            value = new DateOnly(year, month, 1);
            return true;
        }

        int day = int.Parse(match.Groups["day"].Value, CultureInfo.InvariantCulture);
        if (day > DateTime.DaysInMonth(year, month))
        {
            return false;
        }

        precision = DatePrecision.ActualDate;
        value = new DateOnly(year, month, day);
        return true;
    }

    /// <summary>
    /// The error for the answer, or null when it is a real date on the right side of today.
    /// </summary>
    public static string? Check(FormControl control, string answer, DateOnly today)
    {
        if (!TryParse(control, answer, out DatePrecision precision, out DateOnly value))
        {
            return RecordFormMessages.InvalidDate;
        }

        if (precision == DatePrecision.ActualDate)
        {
            return value > today ? RecordFormMessages.ActualDateInFuture : null;
        }

        if (precision == DatePrecision.ActualMonth)
        {
            return value > today ? RecordFormMessages.ActualMonthInFuture : null;
        }

        // The last day is found within the period, as the day after it may be past DateOnly.MaxValue.
        int lastMonth = value.Month + (precision == DatePrecision.EstimatedQuarter ? 2 : 0);
        DateOnly lastDay = new(value.Year, lastMonth, DateTime.DaysInMonth(value.Year, lastMonth));
        return lastDay < today ? RecordFormMessages.EstimatedDateInPast : null;
    }

    /// <summary>
    /// The precision of a <see cref="FormControl.PrecisionDate"/> answer, which is what
    /// conditions on a date compare; null when unanswered or not a real date.
    /// </summary>
    public static string? PrecisionOf(string? answer) =>
        answer is not null
        && TryParse(FormControl.PrecisionDate, answer, out DatePrecision precision, out _)
            ? precision.ToString()
            : null;

    // Years from 1000 so the year is always four digits; the day is checked against the month.
    [GeneratedRegex(
        "^(?<year>[1-9][0-9]{3})-(Q(?<quarter>[1-4])|(?<month>0[1-9]|1[0-2])(-(?<day>0[1-9]|[12][0-9]|3[01]))?)$",
        RegexOptions.ExplicitCapture,
        1000
    )]
    private static partial Regex Pattern();
}
