namespace Finstat.Desktop.Core.ViewModels;

public enum Section
{
    Overview,
    Expenses,
    Categories,
}

public class ShellViewModel : ViewModelBase
{
    private Section section = Section.Overview;

    public ShellViewModel(OverviewViewModel overview, ExpensesViewModel expenses, CategoriesViewModel categories, NotificationCenter notifications)
    {
        Overview = overview;
        Expenses = expenses;
        Categories = categories;
        Notifications = notifications;
        NavigateCommand = new AsyncCommand<string>(NavigateAsync);
        overview.NavigationRequested += NavigateAsync;
        expenses.NavigationRequested += NavigateAsync;
    }

    public OverviewViewModel Overview { get; }

    public ExpensesViewModel Expenses { get; }

    public CategoriesViewModel Categories { get; }

    public NotificationCenter Notifications { get; }

    public AsyncCommand<string> NavigateCommand { get; }

    public Section Section
    {
        get => section;
        private set
        {
            SetProperty(ref section, value);
            OnPropertyChanged(nameof(IsOverview));
            OnPropertyChanged(nameof(IsExpenses));
            OnPropertyChanged(nameof(IsCategories));
        }
    }

    public bool IsOverview => section == Section.Overview;

    public bool IsExpenses => section == Section.Expenses;

    public bool IsCategories => section == Section.Categories;

    public async Task StartAsync()
    {
        await Overview.LoadAsync();
    }

    public async Task NavigateAsync(string name)
    {
        var target = Enum.Parse<Section>(name, true);
        Section = target;
        if (target == Section.Overview)
            await Overview.LoadAsync();
        else if (target == Section.Expenses)
            await Expenses.LoadAsync();
        else
            await Categories.LoadAsync();
    }
}
