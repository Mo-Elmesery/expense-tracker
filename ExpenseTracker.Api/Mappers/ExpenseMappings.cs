using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Mappers;

public static class ExpenseMappings
{
    public static ExpenseDto ToDto(this Expense expense) => new(
        expense.Id,
        expense.Title,
        expense.Amount,
        expense.Category,
        expense.ExpenseDate,
        expense.Notes,
        expense.CreatedAtUtc
    );
}
