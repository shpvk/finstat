using Finstat.Desktop.Core.DTOs;
using Finstat.Desktop.Core.Formatting;

namespace Finstat.Desktop.Core.ViewModels;

public class ExpenseRowViewModel : ViewModelBase
{
    private bool isConfirmingDelete;
    private bool isDeleting;
    private string? deleteError;

    public ExpenseRowViewModel(ExpenseResponseDto expense, CategoryResponseDto? category, bool showDivider)
    {
        Expense = expense;
        ShowDivider = showDivider;
        DayText = DateFormatter.FormatDay(expense.Date);
        WeekdayText = DateFormatter.FormatWeekday(expense.Date);
        AmountText = MoneyFormatter.Format(expense.Amount);
        CategoryName = "Невідома категорія";
        if (category is not null)
        {
            CategoryName = category.Name;
            IsArchivedCategory = category.IsArchived;
        }
        CommentText = "—";
        if (!string.IsNullOrEmpty(expense.Comment))
            CommentText = expense.Comment;
    }

    public ExpenseResponseDto Expense { get; }

    public bool ShowDivider { get; }

    public string DayText { get; }

    public string WeekdayText { get; }

    public string CategoryName { get; }

    public bool IsArchivedCategory { get; }

    public string CommentText { get; }

    public string AmountText { get; }

    public string ConfirmText => "Видалити витрату " + AmountText + " від " + DayText + "?";

    public bool IsConfirmingDelete
    {
        get => isConfirmingDelete;
        set
        {
            if (SetProperty(ref isConfirmingDelete, value))
            {
                DeleteError = null;
                OnPropertyChanged(nameof(IsIdle));
            }
        }
    }

    public bool IsIdle => !isConfirmingDelete;

    public bool IsDeleting
    {
        get => isDeleting;
        set => SetProperty(ref isDeleting, value);
    }

    public string? DeleteError
    {
        get => deleteError;
        set => SetProperty(ref deleteError, value);
    }
}
