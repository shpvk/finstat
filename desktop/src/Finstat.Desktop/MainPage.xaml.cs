using System.ComponentModel;
using Finstat.Desktop.Controls;
using Finstat.Desktop.Core.ViewModels;

namespace Finstat.Desktop;

public partial class MainPage : ContentPage
{
    private static readonly Color AccentText = Color.FromArgb("#1245b9");
    private static readonly Color TextMuted = Color.FromArgb("#545b66");
    private static readonly Color Text = Color.FromArgb("#151b24");

    private const double MaxContentWidth = 1200;
    private const double NavStep = 40;

    private readonly ShellViewModel viewModel;
    private readonly Dictionary<BoxView, double> hoverTargets = new();
    private readonly Dictionary<Label, Color> textTargets = new();
    private Button? hovered;
    private bool toastShown;
    private int shownSection = -1;
    private int switchVersion;
    private View? fadingOut;
    private View? preparing;

    public MainPage(ShellViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelChanged;
        viewModel.Notifications.PropertyChanged += OnNotificationChanged;
        Loaded += async (sender, args) => await viewModel.StartAsync();
        OverviewIcon.Source = "nav_overview.png";
        OverviewIconActive.Source = "nav_overview_active.png";
        ExpensesIcon.Source = "nav_expenses.png";
        ExpensesIconActive.Source = "nav_expenses_active.png";
        CategoriesIcon.Source = "nav_categories.png";
        CategoriesIconActive.Source = "nav_categories_active.png";
        UpdateNavigation(false);
    }

    private void OnContentSizeChanged(object? sender, EventArgs args)
    {
        if (sender is ScrollView scroll && scroll.Width > 0)
            Column.WidthRequest = Math.Min(MaxContentWidth, scroll.Width);
    }

    private async void OnViewModelChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName != nameof(ShellViewModel.Section))
            return;

        var prepared = await PrepareIncoming();
        var before = shownSection;
        UpdateNavigation(true);
        if (preparing == prepared)
            preparing = null;
        EnforceSections();
        if (shownSection != before)
            ActiveButton().Focus();
    }

    private Button ActiveButton()
    {
        return shownSection == 0 ? OverviewItem : shownSection == 1 ? ExpensesItem : CategoriesItem;
    }

    private void EnforceSections()
    {
        foreach (var section in new View[] { OverviewSection, ExpensesSection, CategoriesSection })
        {
            if (section == SectionAt(shownSection) || section == fadingOut || section == preparing)
                continue;
            section.CancelAnimations();
            section.IsVisible = false;
            section.InputTransparent = false;
            section.Opacity = 1;
            section.TranslationY = 0;
            section.ZIndex = 0;
        }
    }

    private async void OnNotificationChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName != nameof(NotificationCenter.IsVisible))
            return;

        var center = viewModel.Notifications;
        if (center.IsVisible)
        {
            ToastText.Text = center.Message;
            if (toastShown)
                return;
            toastShown = true;
            var resume = Toast.IsVisible;
            Toast.IsVisible = true;
            await Motion.Reveal(Toast, 8, resume);
        }
        else
        {
            toastShown = false;
            await Motion.Hide(Toast, 8);
            if (!toastShown)
                Toast.IsVisible = false;
        }
    }

    private void OnNavPointerEntered(object? sender, PointerEventArgs args)
    {
        hovered = sender as Button;
        UpdateNavigation(true);
    }

    private void OnNavPointerExited(object? sender, PointerEventArgs args)
    {
        hovered = null;
        UpdateNavigation(true);
    }

    private async Task<View?> PrepareIncoming()
    {
        var section = viewModel.IsOverview ? 0 : viewModel.IsExpenses ? 1 : 2;
        if (shownSection < 0 || section == shownSection || !Motion.Enabled)
            return null;

        var incoming = SectionAt(section);
        if (incoming == fadingOut)
            return null;

        incoming.Opacity = 0;
        incoming.IsVisible = true;
        preparing = incoming;
        await Task.Delay(16);
        return incoming;
    }

    private View SectionAt(int section)
    {
        return section == 0 ? OverviewSection : section == 1 ? ExpensesSection : CategoriesSection;
    }

    private void UpdateNavigation(bool animate)
    {
        var section = viewModel.IsOverview ? 0 : viewModel.IsExpenses ? 1 : 2;
        var changed = section != shownSection;
        var previous = shownSection;
        shownSection = section;
        if (changed)
        {
            if (animate && previous >= 0)
                _ = SwitchContent(SectionAt(previous), SectionAt(section));
            else
                ShowOnly(SectionAt(section));

            if (animate && Motion.Enabled)
                _ = Pill.TranslateToAsync(0, section * NavStep, Motion.Base, Easing.CubicOut);
            else
                Pill.TranslationY = section * NavStep;
        }

        Apply(OverviewItem, OverviewHover, OverviewIcon, OverviewIconActive, OverviewLabel, section == 0, changed, animate);
        Apply(ExpensesItem, ExpensesHover, ExpensesIcon, ExpensesIconActive, ExpensesLabel, section == 1, changed, animate);
        Apply(CategoriesItem, CategoriesHover, CategoriesIcon, CategoriesIconActive, CategoriesLabel, section == 2, changed, animate);
    }

    private void ShowOnly(View visible)
    {
        foreach (var section in new View[] { OverviewSection, ExpensesSection, CategoriesSection })
        {
            section.CancelAnimations();
            section.InputTransparent = false;
            section.Opacity = 1;
            section.TranslationY = 0;
            section.ZIndex = 0;
            section.IsVisible = section == visible;
        }
    }

    private async Task SwitchContent(View outgoing, View incoming)
    {
        var token = ++switchVersion;
        var settled = incoming == fadingOut && incoming.Opacity >= 1 && incoming.TranslationY == 0;
        fadingOut = outgoing;
        EnforceSections();

        var scrolled = Scroller.ScrollY;
        if (scrolled > 0)
            _ = Scroller.ScrollToAsync(0, 0, false);

        if (!Motion.Enabled)
        {
            fadingOut = null;
            ShowOnly(incoming);
            return;
        }

        outgoing.AbortAnimation("FadeTo");
        outgoing.AbortAnimation("TranslateTo");
        outgoing.InputTransparent = true;
        var fadeOut = Task.CompletedTask;
        if (scrolled > 0)
        {
            outgoing.TranslationY = -scrolled;
            fadeOut = outgoing.FadeToAsync(0, Motion.Base, Easing.CubicOut);
        }
        else if (outgoing.Opacity < 1)
        {
            fadeOut = outgoing.FadeToAsync(0, Motion.Fast, Easing.CubicOut);
        }

        incoming.InputTransparent = false;
        incoming.IsVisible = true;
        if (settled)
        {
            outgoing.ZIndex = 1;
            incoming.ZIndex = 0;
            await fadeOut;
        }
        else
        {
            outgoing.ZIndex = 0;
            incoming.ZIndex = 1;
            incoming.CancelAnimations();
            incoming.Opacity = 0;
            incoming.TranslationY = 4;
            await Task.WhenAll(Motion.Reveal(incoming, 4, true), fadeOut);
        }

        if (token != switchVersion)
            return;
        fadingOut = null;
        EnforceSections();
        incoming.ZIndex = 0;
    }

    private void Apply(Button item, BoxView hover, Image icon, Image iconActive, Label label, bool isActive, bool sectionChanged, bool animate)
    {
        if (sectionChanged)
        {
            if (animate)
            {
                Motion.Fade(icon, "icon", isActive ? 0 : 1, Motion.Base);
                Motion.Fade(iconActive, "icon", isActive ? 1 : 0, Motion.Base);
            }
            else
            {
                icon.Opacity = isActive ? 0 : 1;
                iconActive.Opacity = isActive ? 1 : 0;
            }
        }

        var hoverTarget = !isActive && item == hovered ? 1d : 0d;
        if (!hoverTargets.TryGetValue(hover, out var previous) || previous != hoverTarget)
        {
            hoverTargets[hover] = hoverTarget;
            if (animate)
                Motion.Fade(hover, "hover", hoverTarget, sectionChanged ? Motion.Base : Motion.Fast);
            else
                hover.Opacity = hoverTarget;
        }

        var textTarget = isActive ? AccentText : item == hovered ? Text : TextMuted;
        var known = textTargets.TryGetValue(label, out var current);
        if (known && current!.Equals(textTarget))
            return;
        textTargets[label] = textTarget;
        if (animate && known)
            Motion.Tint(label, "text", label.TextColor, textTarget, color => label.TextColor = color, sectionChanged ? Motion.Base : Motion.Fast);
        else
            label.TextColor = textTarget;
    }
}
