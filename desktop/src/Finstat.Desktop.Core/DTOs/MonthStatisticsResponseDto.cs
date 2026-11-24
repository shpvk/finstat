namespace Finstat.Desktop.Core.DTOs;

public record MonthStatisticsResponseDto(string Month, string Total, List<CategoryStatisticsResponseDto> Categories, List<ExpenseResponseDto> Recent);
