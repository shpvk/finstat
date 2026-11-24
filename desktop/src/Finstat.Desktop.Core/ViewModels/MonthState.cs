using Finstat.Desktop.Core.Formatting;

namespace Finstat.Desktop.Core.ViewModels;

public class MonthState : ViewModelBase
{
    private readonly Func<DateOnly> today;
    private DateOnly month;

    public MonthState(Func<DateOnly> today)
    {
        this.today = today;
        month = FirstDay(today());
        PreviousCommand = new AsyncCommand(() => SetMonthAsync(month.AddMonths(-1)));
        NextCommand = new AsyncCommand(() => SetMonthAsync(month.AddMonths(1)));
        CurrentCommand = new AsyncCommand(() => SetMonthAsync(FirstDay(this.today())));
    }

    public event Func<Task>? Changed;

    public DateOnly Month => month;

    public string Label => DateFormatter.FormatMonth(month);

    public string LabelLower => DateFormatter.FormatMonthLower(month);

    public string ApiMonth => DateFormatter.ToApiMonth(month);

    public bool IsCurrent => month == FirstDay(today());

    public DateOnly Today => today();

    public AsyncCommand PreviousCommand { get; }

    public AsyncCommand NextCommand { get; }

    public AsyncCommand CurrentCommand { get; }

    public async Task SetMonthAsync(DateOnly value)
    {
        var first = FirstDay(value);
        if (first == month)
            return;
        month = first;
        OnPropertyChanged(nameof(Month));
        OnPropertyChanged(nameof(Label));
        OnPropertyChanged(nameof(LabelLower));
        OnPropertyChanged(nameof(ApiMonth));
        OnPropertyChanged(nameof(IsCurrent));
        if (Changed is not null)
            await Changed.Invoke();
    }

    private static DateOnly FirstDay(DateOnly date)
    {
        return new DateOnly(date.Year, date.Month, 1);
    }
}
