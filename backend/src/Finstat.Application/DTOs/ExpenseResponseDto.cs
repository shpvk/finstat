namespace Finstat.Application.DTOs;

public record ExpenseResponseDto(Guid Id, string Amount, Guid CategoryId, DateOnly Date, string? Comment);
