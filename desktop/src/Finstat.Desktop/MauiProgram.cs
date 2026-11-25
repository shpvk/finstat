using Finstat.Desktop.Core.Api;
using Finstat.Desktop.Core.ViewModels;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Finstat.Desktop;

public static class MauiProgram
{
    public const string DefaultApiAddress = "http://localhost:8080/";

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Inter-Regular.ttf", "Inter");
                fonts.AddFont("Inter-Medium.ttf", "InterMedium");
                fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");
            });
        ConfigureNativeControls();

        var apiAddress = Environment.GetEnvironmentVariable("FINSTAT_API_URL");
        if (string.IsNullOrWhiteSpace(apiAddress))
            apiAddress = DefaultApiAddress;
        if (!apiAddress.EndsWith('/'))
            apiAddress += "/";

        var http = new HttpClient { BaseAddress = new Uri(apiAddress), Timeout = TimeSpan.FromSeconds(15) };
        builder.Services.AddSingleton(new FinstatApiClient(http));
        builder.Services.AddSingleton(new MonthState(() => DateOnly.FromDateTime(DateTime.Now)));
        builder.Services.AddSingleton<NotificationCenter>();
        builder.Services.AddSingleton<OverviewViewModel>();
        builder.Services.AddSingleton<ExpensesViewModel>();
        builder.Services.AddSingleton<CategoriesViewModel>();
        builder.Services.AddSingleton<ShellViewModel>();

        return builder.Build();
    }

    private static void ConfigureNativeControls()
    {
        PickerHandler.Mapper.AppendToMapping(nameof(IPicker.Title), ShowTitleAsPlaceholder);
        PickerHandler.Mapper.AppendToMapping(nameof(IPicker.TitleColor), ShowTitleAsPlaceholder);
        DatePickerHandler.Mapper.AppendToMapping("FieldAppearance", (handler, datePicker) =>
        {
            handler.PlatformView.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
            handler.PlatformView.Loaded += HideCalendarGlyph;
        });
    }

    private static void HideCalendarGlyph(object sender, Microsoft.UI.Xaml.RoutedEventArgs args)
    {
        var picker = (Microsoft.UI.Xaml.Controls.CalendarDatePicker)sender;
        picker.ApplyTemplate();
        HideIcons(picker);
    }

    private static void HideIcons(Microsoft.UI.Xaml.DependencyObject parent)
    {
        var count = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent);
        for (var index = 0; index < count; index++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, index);
            if (child is Microsoft.UI.Xaml.Controls.FontIcon icon)
                icon.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            else
                HideIcons(child);
        }
    }

    private static void ShowTitleAsPlaceholder(IPickerHandler handler, IPicker picker)
    {
        handler.PlatformView.Header = null;
        handler.PlatformView.HeaderTemplate = null;
        handler.PlatformView.PlaceholderText = picker.Title;
        handler.PlatformView.PlaceholderForeground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Color.FromArgb("#151b24").ToWindowsColor());
        handler.PlatformView.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
    }
}
