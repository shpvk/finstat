namespace Finstat.Desktop.Core.Formatting;

public static class UkrainianPlural
{
    public static string Choose(int count, string one, string few, string many)
    {
        var lastTwo = count % 100;
        var last = count % 10;
        if (lastTwo >= 11 && lastTwo <= 14)
            return many;
        if (last == 1)
            return one;
        if (last >= 2 && last <= 4)
            return few;
        return many;
    }
}
