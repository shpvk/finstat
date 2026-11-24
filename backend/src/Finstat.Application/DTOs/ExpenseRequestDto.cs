namespace Finstat.Application.DTOs;

public record ExpenseRequestDto(string? Amount, Guid CategoryId, DateOnly? Date, string? Comment);
