namespace Finstat.Desktop.Controls;

public partial class LoadingView : ContentView
{
    private bool isAnimating;

    public LoadingView()
    {
        InitializeComponent();
        PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(IsVisible))
                UpdateAnimation();
        };
        Loaded += (sender, args) => UpdateAnimation();
        Unloaded += (sender, args) => isAnimating = false;
    }

    private async void UpdateAnimation()
    {
        if (!IsVisible || isAnimating)
        {
            if (!IsVisible)
                isAnimating = false;
            return;
        }

        isAnimating = true;
        while (isAnimating)
        {
            await Task.WhenAll(First.FadeToAsync(0.55, 700), Second.FadeToAsync(0.55, 700), Third.FadeToAsync(0.55, 700));
            await Task.WhenAll(First.FadeToAsync(1, 700), Second.FadeToAsync(1, 700), Third.FadeToAsync(1, 700));
        }
    }
}
