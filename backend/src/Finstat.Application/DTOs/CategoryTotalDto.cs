namespace Finstat.Application.DTOs;

public record CategoryTotalDto(Guid CategoryId, string Name, bool IsArchived, decimal Total);
