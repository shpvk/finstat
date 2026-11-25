using Finstat.Desktop.Core.Api;
using Finstat.Desktop.Core.DTOs;
using Finstat.Desktop.Core.Formatting;

namespace Finstat.Desktop.Core.ViewModels;

public class ExpensesViewModel : ViewModelBase
{
    private readonly FinstatApiClient api;
    private readonly NotificationCenter notifications;
    private List<CategoryResponseDto> categories = new();
    private List<ExpenseRowViewModel> rows = new();
    private LoadState state = LoadState.Loading;
    private string errorMessage = "";
    private bool categoriesLoaded;

    public ExpensesViewModel(FinstatApiClient api, MonthState month, NotificationCenter notifications)
    {
        this.api = api;
        this.notifications = notifications;
        Month = month;
        Month.Changed += LoadExpensesAsync;
        Form = new ExpenseFormViewModel(SubmitAsync, () => month.Today);
        ReloadCommand = new AsyncCommand(LoadAsync);
        EditCommand = new AsyncCommand<ExpenseRowViewModel>(EditAsync);
        DeleteCommand = new AsyncCommand<ExpenseRowViewModel>(StartDeleteAsync);
        CancelDeleteCommand = new AsyncCommand<ExpenseRowViewModel>(CancelDeleteAsync);
        ConfirmDeleteCommand = new AsyncCommand<ExpenseRowViewModel>(ConfirmDeleteAsync);
        OpenCategoriesCommand = new AsyncCommand(OpenCategoriesAsync);
    }

    public event Func<string, Task>? NavigationRequested;

    public AsyncCommand OpenCategoriesCommand { get; }

    public MonthState Month { get; }

    public ExpenseFormViewModel Form { get; }

    public AsyncCommand ReloadCommand { get; }

    public AsyncCommand<ExpenseRowViewModel> EditCommand { get; }

    public AsyncCommand<ExpenseRowViewModel> DeleteCommand { get; }

    public AsyncCommand<ExpenseRowViewModel> CancelDeleteCommand { get; }

    public AsyncCommand<ExpenseRowViewModel> ConfirmDeleteCommand { get; }

    public List<ExpenseRowViewModel> Rows
    {
        get => rows;
        private set
        {
            SetProperty(ref rows, value);
            OnPropertyChanged(nameof(CountText));
        }
    }

    public string CountText
    {
        get
        {
            var count = rows.Count;
            if (count == 0)
                return "";
            return count + " " + UkrainianPlural.Choose(count, "витрата", "витрати", "витрат");
        }
    }

    public bool HasActiveCategories
    {
        get
        {
            foreach (var category in categories)
            {
                if (!category.IsArchived)
                    return true;
            }
            return false;
        }
    }

    public bool ShowForm => categoriesLoaded && HasActiveCategories;

    public bool ShowNoCategories => categoriesLoaded && !HasActiveCategories;

    public string NoCategoriesText
    {
        get
        {
            if (categories.Count > 0)
                return "Усі категорії в архіві. Щоб записати витрату, спершу додайте категорію.";
            return "Щоб записати витрату, спершу додайте категорію.";
        }
    }

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

    public async Task LoadAsync()
    {
        State = LoadState.Loading;
        try
        {
            categories = await api.ListCategoriesAsync(true, CancellationToken.None);
            categoriesLoaded = true;
            Form.SetCategories(categories);
            OnPropertyChanged(nameof(HasActiveCategories));
            OnPropertyChanged(nameof(ShowForm));
            OnPropertyChanged(nameof(ShowNoCategories));
            OnPropertyChanged(nameof(NoCategoriesText));
        }
        catch (ApiException exception)
        {
            ErrorMessage = exception.Message;
            State = LoadState.Error;
            return;
        }
        await LoadExpensesAsync();
    }

    public async Task LoadExpensesAsync()
    {
        State = LoadState.Loading;
        var requestedMonth = Month.ApiMonth;
        try
        {
            var expenses = await api.ListExpensesAsync(requestedMonth, CancellationToken.None);
            if (requestedMonth != Month.ApiMonth)
                return;
            Rows = BuildRows(expenses);
            if (expenses.Count == 0)
                State = LoadState.Empty;
            else
                State = LoadState.Ready;
        }
        catch (ApiException exception)
        {
            if (requestedMonth != Month.ApiMonth)
                return;
            ErrorMessage = exception.Message;
            State = LoadState.Error;
        }
    }

    private List<ExpenseRowViewModel> BuildRows(List<ExpenseResponseDto> expenses)
    {
        var byId = new Dictionary<Guid, CategoryResponseDto>();
        foreach (var category in categories)
            byId[category.Id] = category;

        var list = new List<ExpenseRowViewModel>();
        foreach (var expense in expenses)
        {
            byId.TryGetValue(expense.CategoryId, out var category);
            list.Add(new ExpenseRowViewModel(expense, category, list.Count > 0));
        }
        return list;
    }

    private async Task SubmitAsync(ExpenseRequestDto request, ExpenseResponseDto? editing)
    {
        var targetMonth = new DateOnly(request.Date.Year, request.Date.Month, 1);
        if (editing is null)
        {
            await api.CreateExpenseAsync(request, CancellationToken.None);
            Form.Reset(categories);
            if (targetMonth == Month.Month)
            {
                notifications.Show("Витрату додано");
                await LoadExpensesAsync();
                return;
            }
            notifications.Show("Витрату додано за " + DateFormatter.FormatMonthLower(targetMonth));
            await Month.SetMonthAsync(targetMonth);
            return;
        }

        await api.UpdateExpenseAsync(editing.Id, request, CancellationToken.None);
        Form.Reset(categories);
        if (targetMonth == Month.Month)
            notifications.Show("Зміни збережено");
        else
            notifications.Show("Витрату перенесено на " + DateFormatter.FormatMonthLower(targetMonth));
        await LoadExpensesAsync();
    }

    private async Task OpenCategoriesAsync()
    {
        if (NavigationRequested is not null)
            await NavigationRequested.Invoke("Categories");
    }

    private Task EditAsync(ExpenseRowViewModel row)
    {
        Form.StartEdit(row.Expense, categories);
        return Task.CompletedTask;
    }

    private Task StartDeleteAsync(ExpenseRowViewModel row)
    {
        foreach (var other in rows)
            other.IsConfirmingDelete = false;
        row.IsConfirmingDelete = true;
        return Task.CompletedTask;
    }

    private Task CancelDeleteAsync(ExpenseRowViewModel row)
    {
        if (!row.IsDeleting)
            row.IsConfirmingDelete = false;
        return Task.CompletedTask;
    }

    private async Task ConfirmDeleteAsync(ExpenseRowViewModel row)
    {
        row.IsDeleting = true;
        try
        {
            await api.DeleteExpenseAsync(row.Expense.Id, CancellationToken.None);
            notifications.Show("Витрату видалено");
            await LoadExpensesAsync();
        }
        catch (ApiException exception)
        {
            row.DeleteError = exception.Message;
        }
        finally
        {
            row.IsDeleting = false;
        }
    }
}
