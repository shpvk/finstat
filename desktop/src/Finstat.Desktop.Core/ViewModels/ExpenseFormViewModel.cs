using Finstat.Desktop.Core.Api;
using Finstat.Desktop.Core.DTOs;
using Finstat.Desktop.Core.Formatting;

namespace Finstat.Desktop.Core.ViewModels;

public record CategoryOption(Guid Id, string Label);

public class ExpenseFormViewModel : ViewModelBase
{
    public const int MaxCommentLength = 200;

    private readonly Func<ExpenseRequestDto, ExpenseResponseDto?, Task> submit;
    private readonly Func<DateOnly> today;
    private ExpenseResponseDto? editing;
    private List<CategoryOption> options = new();
    private string amountText = "";
    private CategoryOption? selectedCategory;
    private DateTime? date;
    private string comment = "";
    private string? amountError;
    private string? categoryError;
    private string? formError;
    private bool isSaving;

    public ExpenseFormViewModel(Func<ExpenseRequestDto, ExpenseResponseDto?, Task> submit, Func<DateOnly> today)
    {
        this.submit = submit;
        this.today = today;
        date = today().ToDateTime(TimeOnly.MinValue);
        SubmitCommand = new AsyncCommand(SubmitAsync);
        CancelCommand = new AsyncCommand(CancelAsync);
    }

    public AsyncCommand SubmitCommand { get; }

    public AsyncCommand CancelCommand { get; }

    public bool IsEditing => editing is not null;

    public string Title
    {
        get
        {
            if (IsEditing)
                return "Редагування витрати";
            return "Нова витрата";
        }
    }

    public string SubmitText
    {
        get
        {
            if (IsEditing)
                return "Зберегти";
            return "Додати";
        }
    }

    public List<CategoryOption> Options
    {
        get => options;
        private set => SetProperty(ref options, value);
    }

    public string AmountText
    {
        get => amountText;
        set
        {
            if (SetProperty(ref amountText, value))
            {
                AmountError = null;
                FormError = null;
            }
        }
    }

    public CategoryOption? SelectedCategory
    {
        get => selectedCategory;
        set
        {
            if (SetProperty(ref selectedCategory, value))
            {
                CategoryError = null;
                FormError = null;
            }
        }
    }

    public DateTime? Date
    {
        get => date;
        set
        {
            if (SetProperty(ref date, value))
                FormError = null;
        }
    }

    public string Comment
    {
        get => comment;
        set
        {
            if (SetProperty(ref comment, value))
                FormError = null;
        }
    }

    public string? AmountError
    {
        get => amountError;
        private set => SetProperty(ref amountError, value);
    }

    public string? CategoryError
    {
        get => categoryError;
        private set => SetProperty(ref categoryError, value);
    }

    public string? FormError
    {
        get => formError;
        private set => SetProperty(ref formError, value);
    }

    public bool IsSaving
    {
        get => isSaving;
        private set => SetProperty(ref isSaving, value);
    }

    public void SetCategories(List<CategoryResponseDto> categories)
    {
        var list = new List<CategoryOption>();
        foreach (var category in categories)
        {
            if (!category.IsArchived)
                list.Add(new CategoryOption(category.Id, category.Name));
            else if (editing is not null && editing.CategoryId == category.Id)
                list.Add(new CategoryOption(category.Id, category.Name + " (в архіві)"));
        }

        var selectedId = selectedCategory?.Id;
        Options = list;
        SelectedCategory = FindOption(selectedId);
    }

    public void StartEdit(ExpenseResponseDto expense, List<CategoryResponseDto> categories)
    {
        editing = expense;
        SetCategories(categories);
        AmountText = MoneyFormatter.ToInput(expense.Amount);
        SelectedCategory = FindOption(expense.CategoryId);
        Date = expense.Date.ToDateTime(TimeOnly.MinValue);
        Comment = "";
        if (expense.Comment is not null)
            Comment = expense.Comment;
        ClearErrors();
        RaiseModeChanged();
    }

    public void Reset(List<CategoryResponseDto> categories)
    {
        var wasEditing = editing is not null;
        editing = null;
        SetCategories(categories);
        AmountText = "";
        Comment = "";
        if (wasEditing)
        {
            SelectedCategory = null;
            Date = today().ToDateTime(TimeOnly.MinValue);
        }
        ClearErrors();
        RaiseModeChanged();
    }

    private async Task SubmitAsync()
    {
        var parsed = AmountParser.Parse(AmountText);
        AmountError = parsed.Error;
        CategoryError = null;
        if (SelectedCategory is null)
            CategoryError = "Оберіть категорію.";
        if (!parsed.IsValid || SelectedCategory is null || Date is null)
            return;

        var trimmedComment = Comment.Trim();
        string? sentComment = null;
        if (trimmedComment.Length > 0)
            sentComment = trimmedComment;

        var request = new ExpenseRequestDto(
            parsed.Amount!,
            SelectedCategory.Id,
            DateOnly.FromDateTime(Date.Value),
            sentComment);

        IsSaving = true;
        try
        {
            await submit(request, editing);
        }
        catch (ApiException exception)
        {
            FormError = exception.Message;
        }
        finally
        {
            IsSaving = false;
        }
    }

    private Task CancelAsync()
    {
        if (IsSaving)
            return Task.CompletedTask;
        editing = null;
        AmountText = "";
        Comment = "";
        SelectedCategory = null;
        Date = today().ToDateTime(TimeOnly.MinValue);
        ClearErrors();
        RaiseModeChanged();
        return Task.CompletedTask;
    }

    private CategoryOption? FindOption(Guid? id)
    {
        if (id is null)
            return null;
        foreach (var option in options)
        {
            if (option.Id == id)
                return option;
        }
        return null;
    }

    private void ClearErrors()
    {
        AmountError = null;
        CategoryError = null;
        FormError = null;
    }

    private void RaiseModeChanged()
    {
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(SubmitText));
    }
}
