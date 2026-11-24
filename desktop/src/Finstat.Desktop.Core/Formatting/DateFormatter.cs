using System.Globalization;

namespace Finstat.Desktop.Core.Formatting;

public static class DateFormatter
{
    private static readonly CultureInfo Ukrainian = CultureInfo.GetCultureInfo("uk-UA");

    public static string FormatMonth(DateOnly month)
    {
        var name = Ukrainian.DateTimeFormat.MonthNames[month.Month - 1];
        return char.ToUpper(name[0], Ukrainian) + name.Substring(1) + " " + month.Year;
    }

    public static string FormatMonthLower(DateOnly month)
    {
        var name = Ukrainian.DateTimeFormat.MonthNames[month.Month - 1];
        return name + " " + month.Year;
    }

    public static string FormatDay(DateOnly date)
    {
        var name = Ukrainian.DateTimeFormat.MonthGenitiveNames[date.Month - 1];
        return date.Day + " " + name;
    }

    public static string FormatWeekday(DateOnly date)
    {
        return Ukrainian.DateTimeFormat.GetAbbreviatedDayName(date.DayOfWeek);
    }

    public static string ToApiMonth(DateOnly month)
    {
        return month.ToString("yyyy-MM", CultureInfo.InvariantCulture);
    }
}
