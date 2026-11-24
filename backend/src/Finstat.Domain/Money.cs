using System.Globalization;

namespace Finstat.Domain;

public readonly record struct Money
{
    public const decimal Max = 9999999999.99m;

    public Money(decimal value)
    {
        if (value <= 0)
            throw new DomainException("Сума має бути більшою за нуль");
        if (value > Max)
            throw new DomainException($"Сума не може перевищувати {Max.ToString("0.00", CultureInfo.InvariantCulture)}");
        if (decimal.Round(value, 2) != value)
            throw new DomainException("Сума може мати не більше двох знаків після коми");
        Value = value;
    }

    public decimal Value { get; }

    public static Money Parse(string? text)
    {
        if (!decimal.TryParse(text?.Trim(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var value))
            throw new DomainException("Некоректна сума");
        return new Money(value);
    }

    public override string ToString() => Value.ToString("0.00", CultureInfo.InvariantCulture);
}
