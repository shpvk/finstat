namespace Finstat.Desktop.Controls;

public partial class FieldError : ContentView
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(FieldError), "");

    public FieldError()
    {
        InitializeComponent();
        Behaviors.Add(new RevealBehavior());
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}
