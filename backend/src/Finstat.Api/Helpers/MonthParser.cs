using System.Globalization;

namespace Finstat.Api.Helpers;

public static class MonthParser
{
    public const string InvalidMonthMessage = "Вкажіть місяць у форматі РРРР-ММ";

    public static bool TryParse(string? month, out DateOnly first)
    {
        first = default;
        if (month is null)
            return false;
        if (!DateOnly.TryParseExact(month + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out first))
            return false;
        return first.Year != 9999 || first.Month != 12;
    }
}
