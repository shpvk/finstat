using System.Globalization;

namespace Finstat.Desktop.Core.Formatting;

public static class MoneyFormatter
{
    private static readonly CultureInfo Ukrainian = CultureInfo.GetCultureInfo("uk-UA");

    public static string Format(string amount)
    {
        var value = decimal.Parse(amount, NumberStyles.Number, CultureInfo.InvariantCulture);
        return value.ToString("N2", Ukrainian) + " грн";
    }

    public static string FormatShare(string share)
    {
        var value = decimal.Parse(share, NumberStyles.Number, CultureInfo.InvariantCulture);
        return value.ToString("0.0", Ukrainian) + "%";
    }

    public static string ToInput(string amount)
    {
        return amount.Replace('.', ',');
    }

    public static int CompareAmounts(string left, string right)
    {
        var a = decimal.Parse(left, NumberStyles.Number, CultureInfo.InvariantCulture);
        var b = decimal.Parse(right, NumberStyles.Number, CultureInfo.InvariantCulture);
        return a.CompareTo(b);
    }

    public static int ShareInTenths(string share)
    {
        var value = decimal.Parse(share, NumberStyles.Number, CultureInfo.InvariantCulture);
        return (int)decimal.Round(value * 10);
    }
}
