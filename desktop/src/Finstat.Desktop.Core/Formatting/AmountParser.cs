using System.Text.RegularExpressions;

namespace Finstat.Desktop.Core.Formatting;

public record AmountParseResult(string? Amount, string? Error)
{
    public bool IsValid => Error is null;
}

public static partial class AmountParser
{
    private const int MaxIntegerDigits = 10;

    public static AmountParseResult Parse(string? input)
    {
        if (input is null)
            return Invalid("Вкажіть суму.");

        var value = Regex.Replace(input, @"\s", "").Replace(',', '.');
        if (value.Length == 0)
            return Invalid("Вкажіть суму.");

        var match = AmountPattern().Match(value);
        if (!match.Success)
            return Invalid("Вкажіть суму числом, наприклад 125,50.");

        var integer = match.Groups["integer"].Value.TrimStart('0');
        var fraction = match.Groups["fraction"].Value;
        if (fraction.Length > 2)
            return Invalid("Сума може мати не більше двох знаків після коми.");
        if (integer.Length == 0)
            integer = "0";
        if (integer.Trim('0').Length == 0 && fraction.Trim('0').Length == 0)
            return Invalid("Сума має бути більшою за нуль.");
        if (integer.Length > MaxIntegerDigits)
            return Invalid("Сума завелика.");

        if (fraction.Length == 0)
            return new AmountParseResult(integer, null);
        return new AmountParseResult($"{integer}.{fraction}", null);
    }

    private static AmountParseResult Invalid(string error)
    {
        return new AmountParseResult(null, error);
    }

    [GeneratedRegex(@"^(?<integer>\d+)(\.(?<fraction>\d*))?$")]
    private static partial Regex AmountPattern();
}
