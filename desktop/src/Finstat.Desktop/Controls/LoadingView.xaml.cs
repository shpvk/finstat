namespace Finstat.Desktop.Controls;

public partial class LoadingView : ContentView
{
    private int generation;
    private bool running;

    public LoadingView()
    {
        InitializeComponent();
        PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(IsVisible))
                UpdateAnimation();
        };
        Loaded += (sender, args) => UpdateAnimation();
        Unloaded += (sender, args) => Stop();
    }

    private void Stop()
    {
        generation++;
        running = false;
        foreach (var dot in new View[] { First, Second, Third })
        {
            dot.CancelAnimations();
            dot.Opacity = 1;
        }
    }

    private async void UpdateAnimation()
    {
        if (!IsVisible || !Motion.Enabled)
        {
            Stop();
            return;
        }

        if (running)
            return;

        running = true;
        var mine = ++generation;
        while (mine == generation)
        {
            var cycle = System.Diagnostics.Stopwatch.StartNew();
            await Task.WhenAll(First.FadeToAsync(0.55, 700), Second.FadeToAsync(0.55, 700), Third.FadeToAsync(0.55, 700));
            if (mine != generation)
                return;
            await Task.WhenAll(First.FadeToAsync(1, 700), Second.FadeToAsync(1, 700), Third.FadeToAsync(1, 700));
            if (mine != generation)
                return;
            if (cycle.ElapsedMilliseconds < 1400)
                await Task.Delay(1400 - (int)cycle.ElapsedMilliseconds);
        }
    }
}
