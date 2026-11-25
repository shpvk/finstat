using Finstat.Desktop.Core.Api;
using Finstat.Desktop.Core.DTOs;
using Finstat.Desktop.Core.Formatting;

namespace Finstat.Desktop.Core.ViewModels;

public record RecentItem(string CategoryName, string MetaText, string AmountText, bool ShowDivider);

public class OverviewViewModel : ViewModelBase
{
    private readonly FinstatApiClient api;
    private LoadState state = LoadState.Loading;
    private int loadVersion;
    private string errorMessage = "";
    private string totalText = "";
    private string categoriesCountText = "";
    private string topCategoryName = "";
    private string topCategoryShare = "";
    private List<RecentItem> recent = new();
    private CategoryBreakdown? breakdown;
    private string totalValue = "0.00";
    private string? activeSegment;

    public OverviewViewModel(FinstatApiClient api, MonthState month)
    {
        this.api = api;
        Month = month;
        Month.Changed += LoadAsync;
        ReloadCommand = new AsyncCommand(LoadAsync);
        OpenExpensesCommand = new AsyncCommand(OpenExpensesAsync);
        HoverRowCommand = new AsyncCommand<BreakdownRow>(HoverRowAsync);
        ClearHoverCommand = new AsyncCommand(ClearHoverAsync);
    }

    public event Func<string, Task>? NavigationRequested;

    public AsyncCommand OpenExpensesCommand { get; }

    public AsyncCommand<BreakdownRow> HoverRowCommand { get; }

    public AsyncCommand ClearHoverCommand { get; }

    public MonthState Month { get; }

    public AsyncCommand ReloadCommand { get; }

    public LoadState State
    {
        get => state;
        private set
        {
            SetProperty(ref state, value);
            OnPropertyChanged(nameof(IsLoading));
            OnPropertyChanged(nameof(IsReady));
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(IsError));
        }
    }

    public bool IsLoading => state == LoadState.Loading;

    public bool IsReady => state == LoadState.Ready;

    public bool IsEmpty => state == LoadState.Empty;

    public bool IsError => state == LoadState.Error;

    public string ErrorMessage
    {
        get => errorMessage;
        private set => SetProperty(ref errorMessage, value);
    }

    public string TotalLabel => "Витрачено за " + Month.LabelLower;

    public string EmptyTitle => "За " + Month.LabelLower + " витрат немає";

    public string TotalText
    {
        get => totalText;
        private set => SetProperty(ref totalText, value);
    }

    public string CategoriesCountText
    {
        get => categoriesCountText;
        private set => SetProperty(ref categoriesCountText, value);
    }

    public string TopCategoryName
    {
        get => topCategoryName;
        private set => SetProperty(ref topCategoryName, value);
    }

    public string TopCategoryShare
    {
        get => topCategoryShare;
        private set => SetProperty(ref topCategoryShare, value);
    }

    public List<RecentItem> Recent
    {
        get => recent;
        private set => SetProperty(ref recent, value);
    }

    public List<ChartSegment> Segments
    {
        get
        {
            if (breakdown is null)
                return new List<ChartSegment>();
            return breakdown.Segments;
        }
    }

    public List<BreakdownRow> Rows
    {
        get
        {
            if (breakdown is null)
                return new List<BreakdownRow>();
            return breakdown.Rows;
        }
    }

    public string? ActiveSegment
    {
        get => activeSegment;
        set
        {
            if (SetProperty(ref activeSegment, value))
            {
                foreach (var row in Rows)
                    row.IsActive = value is not null && row.SegmentKey == value;
                OnPropertyChanged(nameof(CenterValue));
                OnPropertyChanged(nameof(CenterLabel));
            }
        }
    }

    public string CenterValue
    {
        get
        {
            var segment = FindActiveSegment();
            if (segment is null)
                return MoneyFormatter.Format(totalValue);
            return CategoryBreakdown.TenthsToShare(segment.Tenths);
        }
    }

    public string CenterLabel
    {
        get
        {
            var segment = FindActiveSegment();
            if (segment is null)
                return "усього";
            return segment.Label;
        }
    }

    public Task LoadAsync()
    {
        return LoadAsync(false);
    }

    public Task RefreshAsync()
    {
        return LoadAsync(true);
    }

    private async Task LoadAsync(bool keepContent)
    {
        var version = ++loadVersion;
        if (!keepContent || state != LoadState.Ready)
            State = LoadState.Loading;
        OnPropertyChanged(nameof(TotalLabel));
        OnPropertyChanged(nameof(EmptyTitle));
        var requestedMonth = Month.ApiMonth;
        try
        {
            var statistics = await api.GetStatisticsAsync(requestedMonth, CancellationToken.None);
            if (requestedMonth != Month.ApiMonth || version != loadVersion)
                return;
            Apply(statistics);
        }
        catch (ApiException exception)
        {
            if (requestedMonth != Month.ApiMonth || version != loadVersion)
                return;
            ErrorMessage = exception.Message;
            State = LoadState.Error;
        }
    }

    private void Apply(MonthStatisticsResponseDto statistics)
    {
        if (statistics.Categories.Count == 0)
        {
            breakdown = null;
            State = LoadState.Empty;
            return;
        }

        totalValue = statistics.Total;
        TotalText = MoneyFormatter.Format(statistics.Total);
        var count = statistics.Categories.Count;
        CategoriesCountText = count + " " + UkrainianPlural.Choose(count, "категорія", "категорії", "категорій");

        var sorted = CategoryBreakdown.SortByTotal(statistics.Categories);
        TopCategoryName = sorted[0].Name;
        TopCategoryShare = MoneyFormatter.FormatShare(sorted[0].Share);

        var names = new Dictionary<Guid, string>();
        foreach (var category in statistics.Categories)
            names[category.CategoryId] = category.Name;

        var items = new List<RecentItem>();
        foreach (var expense in statistics.Recent)
            items.Add(ToRecentItem(expense, names, items.Count > 0));
        Recent = items;

        breakdown = CategoryBreakdown.Build(statistics.Categories);
        activeSegment = null;
        OnPropertyChanged(nameof(Segments));
        OnPropertyChanged(nameof(Rows));
        OnPropertyChanged(nameof(ActiveSegment));
        OnPropertyChanged(nameof(CenterValue));
        OnPropertyChanged(nameof(CenterLabel));
        State = LoadState.Ready;
    }

    private static RecentItem ToRecentItem(ExpenseResponseDto expense, Dictionary<Guid, string> names, bool showDivider)
    {
        var name = "Невідома категорія";
        if (names.TryGetValue(expense.CategoryId, out var found))
            name = found;
        var meta = DateFormatter.FormatDay(expense.Date);
        if (!string.IsNullOrEmpty(expense.Comment))
            meta = meta + " · " + expense.Comment;
        return new RecentItem(name, meta, MoneyFormatter.Format(expense.Amount), showDivider);
    }

    private async Task OpenExpensesAsync()
    {
        if (NavigationRequested is not null)
            await NavigationRequested.Invoke("Expenses");
    }

    private Task HoverRowAsync(BreakdownRow row)
    {
        if (row.HasSegment)
            ActiveSegment = row.SegmentKey;
        return Task.CompletedTask;
    }

    private Task ClearHoverAsync()
    {
        ActiveSegment = null;
        return Task.CompletedTask;
    }

    private ChartSegment? FindActiveSegment()
    {
        if (activeSegment is null || breakdown is null)
            return null;
        foreach (var segment in breakdown.Segments)
        {
            if (segment.Key == activeSegment)
                return segment;
        }
        return null;
    }
}
