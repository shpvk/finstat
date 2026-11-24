using Finstat.Desktop.Core.DTOs;

namespace Finstat.Desktop.Core.ViewModels;

public class CategoryRowViewModel : ViewModelBase
{
    private bool isEditing;
    private bool isConfirmingArchive;
    private bool isSaving;
    private string editName;
    private string? error;

    public CategoryRowViewModel(CategoryResponseDto category, bool showDivider)
    {
        Category = category;
        ShowDivider = showDivider;
        editName = category.Name;
    }

    public CategoryResponseDto Category { get; }

    public bool ShowDivider { get; }

    public string Name => Category.Name;

    public string ArchiveQuestion => "Архівувати «" + Category.Name + "»?";

    public bool IsEditing
    {
        get => isEditing;
        set
        {
            if (SetProperty(ref isEditing, value))
                OnPropertyChanged(nameof(IsIdle));
        }
    }

    public bool IsConfirmingArchive
    {
        get => isConfirmingArchive;
        set
        {
            if (SetProperty(ref isConfirmingArchive, value))
                OnPropertyChanged(nameof(IsIdle));
        }
    }

    public bool IsIdle => !isEditing && !isConfirmingArchive;

    public bool IsSaving
    {
        get => isSaving;
        set => SetProperty(ref isSaving, value);
    }

    public string EditName
    {
        get => editName;
        set
        {
            if (SetProperty(ref editName, value))
                Error = null;
        }
    }

    public string? Error
    {
        get => error;
        set => SetProperty(ref error, value);
    }
}
