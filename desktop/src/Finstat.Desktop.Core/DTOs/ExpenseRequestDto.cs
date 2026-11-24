namespace Finstat.Desktop.Core.DTOs;

public record ExpenseRequestDto(string Amount, Guid CategoryId, DateOnly Date, string? Comment);
