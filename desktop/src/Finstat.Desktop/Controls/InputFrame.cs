using Microsoft.Maui.Controls.Shapes;

namespace Finstat.Desktop.Controls;

public class InputFrame : Border
{
    public static readonly BindableProperty HasErrorProperty = BindableProperty.Create(
        nameof(HasError), typeof(bool), typeof(InputFrame), false, propertyChanged: OnStateChanged);

    private bool isFocused;

    public InputFrame()
    {
        StrokeThickness = 1;
        StrokeShape = new RoundRectangle { CornerRadius = 6 };
        HeightRequest = 40;
        BackgroundColor = Colors.White;
        Padding = 0;
        UpdateStroke();
    }

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == nameof(Content))
            TrackFocus(Content);
    }

    private void TrackFocus(IView? view)
    {
        if (view is Layout layout)
        {
            foreach (var child in layout)
                TrackFocus(child);
            return;
        }

        if (view is not VisualElement element)
            return;
        element.Focused += (sender, args) =>
        {
            isFocused = true;
            UpdateStroke();
        };
        element.Unfocused += (sender, args) =>
        {
            isFocused = false;
            UpdateStroke();
        };
    }

    private static void OnStateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((InputFrame)bindable).UpdateStroke();
    }

    private void UpdateStroke()
    {
        if (isFocused)
        {
            Stroke = Color.FromArgb("#1f5ed9");
            StrokeThickness = 2;
            return;
        }
        StrokeThickness = 1;
        if (HasError)
            Stroke = Color.FromArgb("#d33b36");
        else
            Stroke = Color.FromArgb("#818a95");
    }
}
