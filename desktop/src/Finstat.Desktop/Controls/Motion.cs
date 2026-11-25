using Windows.UI.ViewManagement;

namespace Finstat.Desktop.Controls;

public static class Motion
{
    public const uint Fast = 150;
    public const uint Base = 180;
    public const uint Ring = 600;

    private static readonly UISettings Settings = new();

    public static bool Enabled
    {
        get
        {
            try
            {
                return Settings.AnimationsEnabled;
            }
            catch (Exception)
            {
                return true;
            }
        }
    }

    public static async Task Reveal(VisualElement element, double rise = 0, bool resume = false)
    {
        element.CancelAnimations();
        if (!Enabled)
        {
            element.Opacity = 1;
            element.TranslationY = 0;
            return;
        }

        if (!resume)
        {
            element.Opacity = 0;
            element.TranslationY = rise;
        }

        await Task.WhenAll(
            element.FadeToAsync(1, Base, Easing.CubicOut),
            element.TranslateToAsync(0, 0, Base, Easing.CubicOut));
    }

    public static async Task Hide(VisualElement element, double drop = 0)
    {
        element.CancelAnimations();
        if (!Enabled)
        {
            element.Opacity = 0;
            return;
        }

        await Task.WhenAll(
            element.FadeToAsync(0, Fast, Easing.CubicOut),
            element.TranslateToAsync(0, drop, Fast, Easing.CubicOut));
    }

    public static void Fade(VisualElement element, string name, double to, uint length = Fast)
    {
        element.AbortAnimation(name);
        if (!Enabled)
        {
            element.Opacity = to;
            return;
        }

        var from = element.Opacity;
        new Animation(t => element.Opacity = from + (to - from) * t, 0, 1, Easing.CubicOut)
            .Commit(element, name, 16, length);
    }

    public static void Tint(VisualElement owner, string name, Color from, Color to, Action<Color> apply, uint length = Fast)
    {
        owner.AbortAnimation(name);
        if (!Enabled)
        {
            apply(to);
            return;
        }

        new Animation(t => apply(Mix(from, to, (float)t)), 0, 1, Easing.CubicOut)
            .Commit(owner, name, 16, length);
    }

    public static Color Mix(Color from, Color to, float t)
    {
        return new Color(
            from.Red + (to.Red - from.Red) * t,
            from.Green + (to.Green - from.Green) * t,
            from.Blue + (to.Blue - from.Blue) * t,
            from.Alpha + (to.Alpha - from.Alpha) * t);
    }
}
