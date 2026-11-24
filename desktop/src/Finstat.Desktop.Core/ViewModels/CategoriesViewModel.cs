using Finstat.Desktop.Core.Api;
using Finstat.Desktop.Core.DTOs;

namespace Finstat.Desktop.Core.ViewModels;

public record ArchivedCategoryItem(string Name, bool ShowDivider);

public class CategoriesViewModel : ViewModelBase
{
    public const int MaxNameLength = 50;

    private readonly FinstatApiClient api;
    private readonly NotificationCenter notifications;
    private List<CategoryRowViewModel> active = new();
    private List<ArchivedCategoryItem> archived = new();
    private LoadState state = LoadState.Loading;
    private string errorMessage = "";
    private string newName = "";
    private string? newNameError;
    private bool isAdding;

    public CategoriesViewModel(FinstatApiClient api, NotificationCenter notifications)
    {
        this.api = api;
        this.notifications = notifications;
        ReloadCommand = new AsyncCommand(LoadAsync);
        AddCommand = new AsyncCommand(AddAsync);
        StartRenameCommand = new AsyncCommand<CategoryRowViewModel>(StartRenameAsync);
        CancelRowCommand = new AsyncCommand<CategoryRowViewModel>(CancelRowAsync);
        SaveRenameCommand = new AsyncCommand<CategoryRowViewModel>(SaveRenameAsync);
        StartArchiveCommand = new AsyncCommand<CategoryRowViewModel>(StartArchiveAsync);
        ConfirmArchiveCommand = new AsyncCommand<CategoryRowViewModel>(ConfirmArchiveAsync);
    }

    public AsyncCommand ReloadCommand { get; }

    public AsyncCommand AddCommand { get; }

    public AsyncCommand<CategoryRowViewModel> StartRenameCommand { get; }

    public AsyncCommand<CategoryRowViewModel> CancelRowCommand { get; }

    public AsyncCommand<CategoryRowViewModel> SaveRenameCommand { get; }

    public AsyncCommand<CategoryRowViewModel> StartArchiveCommand { get; }

    public AsyncCommand<CategoryRowViewModel> ConfirmArchiveCommand { get; }

    public List<CategoryRowViewModel> Active
    {
        get => active;
        private set
        {
            SetProperty(ref active, value);
            OnPropertyChanged(nameof(ActiveCount));
            OnPropertyChanged(nameof(HasActive));
            OnPropertyChanged(nameof(HasNoActive));
        }
    }

    public List<ArchivedCategoryItem> Archived
    {
        get => archived;
        private set
        {
            SetProperty(ref archived, value);
            OnPropertyChanged(nameof(ArchivedCount));
            OnPropertyChanged(nameof(HasArchived));
        }
    }

    public int ActiveCount => active.Count;

    public int ArchivedCount => archived.Count;

    public bool HasActive => active.Count > 0;

    public bool HasNoActive => active.Count == 0;

    public bool HasArchived => archived.Count > 0;

    public LoadState State
    {
        get => state;
        private set
        {
            SetProperty(ref state, value);
            OnPropertyChanged(nameof(IsLoading));
            OnPropertyChanged(nameof(IsReady));
            OnPropertyChanged(nameof(IsError));
        }
    }

    public bool IsLoading => state == LoadState.Loading;

    public bool IsReady => state == LoadState.Ready;

    public bool IsError => state == LoadState.Error;

    public string ErrorMessage
    {
        get => errorMessage;
        private set => SetProperty(ref errorMessage, value);
    }

    public string NewName
    {
        get => newName;
        set
        {
            if (SetProperty(ref newName, value))
                NewNameError = null;
        }
    }

    public string? NewNameError
    {
        get => newNameError;
        private set => SetProperty(ref newNameError, value);
    }

    public bool IsAdding
    {
        get => isAdding;
        private set => SetProperty(ref isAdding, value);
    }

    public static string? Validate(string? name)
    {
        if (name is null)
            return "Вкажіть назву категорії.";
        var trimmed = name.Trim();
        if (trimmed.Length == 0)
            return "Вкажіть назву категорії.";
        if (trimmed.Length > MaxNameLength)
            return $"Назва не може бути довшою за {MaxNameLength} символів.";
        return null;
    }

    public async Task LoadAsync()
    {
        if (state != LoadState.Ready)
            State = LoadState.Loading;
        try
        {
            var all = await api.ListCategoriesAsync(true, CancellationToken.None);
            var activeRows = new List<CategoryRowViewModel>();
            var archivedList = new List<ArchivedCategoryItem>();
            foreach (var category in all)
            {
                if (category.IsArchived)
                    archivedList.Add(new ArchivedCategoryItem(category.Name, archivedList.Count > 0));
                else
                    activeRows.Add(new CategoryRowViewModel(category, activeRows.Count > 0));
            }
            Active = activeRows;
            Archived = archivedList;
            State = LoadState.Ready;
        }
        catch (ApiException exception)
        {
            ErrorMessage = exception.Message;
            State = LoadState.Error;
        }
    }

    private async Task AddAsync()
    {
        var error = Validate(NewName);
        if (error is not null)
        {
            NewNameError = error;
            return;
        }

        IsAdding = true;
        try
        {
            var created = await api.CreateCategoryAsync(NewName.Trim(), CancellationToken.None);
            NewName = "";
            notifications.Show("Категорію «" + created.Name + "» додано");
            await LoadAsync();
        }
        catch (ApiException exception)
        {
            NewNameError = exception.Message;
        }
        finally
        {
            IsAdding = false;
        }
    }

    private Task StartRenameAsync(CategoryRowViewModel row)
    {
        CloseOtherRows(row);
        row.EditName = row.Name;
        row.Error = null;
        row.IsEditing = true;
        return Task.CompletedTask;
    }

    private Task StartArchiveAsync(CategoryRowViewModel row)
    {
        CloseOtherRows(row);
        row.Error = null;
        row.IsConfirmingArchive = true;
        return Task.CompletedTask;
    }

    private Task CancelRowAsync(CategoryRowViewModel row)
    {
        if (row.IsSaving)
            return Task.CompletedTask;
        row.IsEditing = false;
        row.IsConfirmingArchive = false;
        row.Error = null;
        return Task.CompletedTask;
    }

    private async Task SaveRenameAsync(CategoryRowViewModel row)
    {
        var error = Validate(row.EditName);
        if (error is not null)
        {
            row.Error = error;
            return;
        }
        if (row.EditName.Trim() == row.Name)
        {
            row.IsEditing = false;
            return;
        }

        row.IsSaving = true;
        try
        {
            var renamed = await api.RenameCategoryAsync(row.Category.Id, row.EditName.Trim(), CancellationToken.None);
            notifications.Show("Категорію перейменовано на «" + renamed.Name + "»");
            await LoadAsync();
        }
        catch (ApiException exception)
        {
            row.Error = exception.Message;
        }
        finally
        {
            row.IsSaving = false;
        }
    }

    private async Task ConfirmArchiveAsync(CategoryRowViewModel row)
    {
        row.IsSaving = true;
        try
        {
            await api.ArchiveCategoryAsync(row.Category.Id, CancellationToken.None);
            notifications.Show("Категорію «" + row.Name + "» перенесено в архів");
            await LoadAsync();
        }
        catch (ApiException exception)
        {
            row.Error = exception.Message;
        }
        finally
        {
            row.IsSaving = false;
        }
    }

    private void CloseOtherRows(CategoryRowViewModel row)
    {
        foreach (var other in active)
        {
            if (other == row || other.IsSaving)
                continue;
            other.IsEditing = false;
            other.IsConfirmingArchive = false;
        }
    }
}
