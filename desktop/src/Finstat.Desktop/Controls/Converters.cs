using System.Globalization;

namespace Finstat.Desktop.Controls;

public class InvertBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool flag && !flag;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool flag && !flag;
    }
}

public class HasTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is string text && text.Length > 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class ChartColorConverter : IValueConverter
{
    private static readonly string[] Palette = { "#2a78d6", "#eb6834", "#1baf7a", "#eda100", "#e87ba4", "#008300" };

    public static Color ColorAt(int index)
    {
        return Color.FromArgb(Palette[index % Palette.Length]);
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int index)
            return ColorAt(index);
        return Colors.Transparent;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
