namespace Finstat.Desktop.Core.ViewModels;

public class NotificationCenter : ViewModelBase
{
    private string? message;
    private int version;

    public string? Message
    {
        get => message;
        private set
        {
            SetProperty(ref message, value);
            OnPropertyChanged(nameof(IsVisible));
        }
    }

    public bool IsVisible => message is not null;

    public async void Show(string text)
    {
        version++;
        var shown = version;
        Message = text;
        await Task.Delay(TimeSpan.FromSeconds(3.5));
        if (shown == version)
            Message = null;
    }
}
