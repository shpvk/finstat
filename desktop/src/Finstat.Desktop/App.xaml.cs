using Finstat.Desktop.Core.ViewModels;

namespace Finstat.Desktop;

public partial class App : Application
{
    private readonly ShellViewModel shell;

    public App(ShellViewModel shell)
    {
        InitializeComponent();
        this.shell = shell;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new MainPage(shell))
        {
            Title = "Finstat",
            Width = 1320,
            Height = 860,
            MinimumWidth = 1100,
            MinimumHeight = 640,
        };
        return window;
    }
}
