namespace ExpenseTracker.Api.Models;

public record CategorySummaryDto(string Category, decimal TotalAmount, int Count);

public record ExpenseSummaryDto(decimal TotalAmount, int Count, IReadOnlyList<CategorySummaryDto> Breakdown);
