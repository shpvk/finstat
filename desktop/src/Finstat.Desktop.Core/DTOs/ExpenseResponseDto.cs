namespace Finstat.Desktop.Core.DTOs;

public record ExpenseResponseDto(Guid Id, string Amount, Guid CategoryId, DateOnly Date, string? Comment);
