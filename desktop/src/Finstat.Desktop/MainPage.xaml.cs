using System.ComponentModel;
using Finstat.Desktop.Core.ViewModels;

namespace Finstat.Desktop;

public partial class MainPage : ContentPage
{
    private static readonly Color AccentSubtle = Color.FromArgb("#e7f1ff");
    private static readonly Color SurfaceHover = Color.FromArgb("#ebeff4");
    private static readonly Color AccentText = Color.FromArgb("#1245b9");
    private static readonly Color TextMuted = Color.FromArgb("#545b66");
    private static readonly Color Text = Color.FromArgb("#151b24");

    private readonly ShellViewModel viewModel;
    private Button? hovered;

    public MainPage(ShellViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelChanged;
        Loaded += async (sender, args) => await viewModel.StartAsync();
        UpdateNavigation();
    }

    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(ShellViewModel.Section))
            UpdateNavigation();
    }

    private void OnNavPointerEntered(object? sender, PointerEventArgs args)
    {
        hovered = sender as Button;
        UpdateNavigation();
    }

    private void OnNavPointerExited(object? sender, PointerEventArgs args)
    {
        hovered = null;
        UpdateNavigation();
    }

    private void UpdateNavigation()
    {
        OverviewSection.IsVisible = viewModel.IsOverview;
        ExpensesSection.IsVisible = viewModel.IsExpenses;
        CategoriesSection.IsVisible = viewModel.IsCategories;
        Apply(OverviewItem, OverviewIcon, OverviewLabel, "nav_overview", viewModel.IsOverview);
        Apply(ExpensesItem, ExpensesIcon, ExpensesLabel, "nav_expenses", viewModel.IsExpenses);
        Apply(CategoriesItem, CategoriesIcon, CategoriesLabel, "nav_categories", viewModel.IsCategories);
    }

    private void Apply(Button item, Image icon, Label label, string iconName, bool isActive)
    {
        if (isActive)
        {
            item.BackgroundColor = AccentSubtle;
            icon.Source = iconName + "_active.png";
            label.TextColor = AccentText;
            return;
        }

        icon.Source = iconName + ".png";
        if (item == hovered)
        {
            item.BackgroundColor = SurfaceHover;
            label.TextColor = Text;
        }
        else
        {
            item.BackgroundColor = Colors.Transparent;
            label.TextColor = TextMuted;
        }
    }
}
