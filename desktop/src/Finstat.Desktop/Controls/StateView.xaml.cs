using System.Windows.Input;

namespace Finstat.Desktop.Controls;

public partial class StateView : ContentView
{
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(string), typeof(StateView), "icon_inbox.png");

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(StateView), "");

    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description), typeof(string), typeof(StateView), "");

    public static readonly BindableProperty ActionTextProperty = BindableProperty.Create(
        nameof(ActionText), typeof(string), typeof(StateView), "");

    public static readonly BindableProperty ActionCommandProperty = BindableProperty.Create(
        nameof(ActionCommand), typeof(ICommand), typeof(StateView));

    public static readonly BindableProperty ActionParameterProperty = BindableProperty.Create(
        nameof(ActionParameter), typeof(object), typeof(StateView));

    public static readonly BindableProperty IsPrimaryActionProperty = BindableProperty.Create(
        nameof(IsPrimaryAction), typeof(bool), typeof(StateView), false, propertyChanged: OnActionStyleChanged);

    public StateView()
    {
        InitializeComponent();
        Behaviors.Add(new RevealBehavior());
        ApplyActionStyle();
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string ActionText
    {
        get => (string)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public ICommand? ActionCommand
    {
        get => (ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public object? ActionParameter
    {
        get => GetValue(ActionParameterProperty);
        set => SetValue(ActionParameterProperty, value);
    }

    public bool IsPrimaryAction
    {
        get => (bool)GetValue(IsPrimaryActionProperty);
        set => SetValue(IsPrimaryActionProperty, value);
    }

    private static void OnActionStyleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((StateView)bindable).ApplyActionStyle();
    }

    private void ApplyActionStyle()
    {
        var key = "SecondaryButton";
        if (IsPrimaryAction)
            key = "PrimaryButton";
        if (Application.Current is not null && Application.Current.Resources.TryGetValue(key, out var style))
            ActionButton.Style = (Style)style;
    }
}
