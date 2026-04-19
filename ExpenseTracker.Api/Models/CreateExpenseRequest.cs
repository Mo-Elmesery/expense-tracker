namespace ExpenseTracker.Api.Models;

public record CreateExpenseRequest(
    string Title,
    decimal Amount,
    string Category,
    DateOnly ExpenseDate,
    string? Notes
);
