namespace Finstat.Desktop.Controls;

public class RevealBehavior : Behavior<VisualElement>
{
    protected override void OnAttachedTo(VisualElement bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.PropertyChanged += OnChanged;
    }

    protected override void OnDetachingFrom(VisualElement bindable)
    {
        bindable.PropertyChanged -= OnChanged;
        base.OnDetachingFrom(bindable);
    }

    private static void OnChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(VisualElement.IsVisible) && sender is VisualElement { IsVisible: true } element)
            _ = Motion.Reveal(element);
    }
}

public class TintBehavior : Behavior<VisualElement>
{
    public static readonly BindableProperty IsActiveProperty = BindableProperty.Create(
        nameof(IsActive), typeof(bool), typeof(TintBehavior), false, propertyChanged: OnActiveChanged);

    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color), typeof(Color), typeof(TintBehavior), Colors.Transparent);

    private VisualElement? element;

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    protected override void OnAttachedTo(VisualElement bindable)
    {
        base.OnAttachedTo(bindable);
        element = bindable;
        BindingContext = bindable.BindingContext;
        bindable.BindingContextChanged += OnContextChanged;
        bindable.BackgroundColor = IsActive ? Color : Color.WithAlpha(0);
    }

    protected override void OnDetachingFrom(VisualElement bindable)
    {
        bindable.BindingContextChanged -= OnContextChanged;
        element = null;
        base.OnDetachingFrom(bindable);
    }

    private void OnContextChanged(object? sender, EventArgs args)
    {
        BindingContext = element?.BindingContext;
    }

    private static void OnActiveChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var behavior = (TintBehavior)bindable;
        var target = behavior.element;
        if (target is null)
            return;

        var to = (bool)newValue ? behavior.Color : behavior.Color.WithAlpha(0);
        var from = target.BackgroundColor ?? behavior.Color.WithAlpha(0);
        Motion.Tint(target, "tint", from, to, color => target.BackgroundColor = color);
    }
}
