namespace Finstat.Desktop.Core.DTOs;

public record CategoryStatisticsResponseDto(Guid CategoryId, string Name, bool IsArchived, string Total, string Share);
