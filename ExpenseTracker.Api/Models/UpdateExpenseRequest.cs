namespace ExpenseTracker.Api.Models;

public record UpdateExpenseRequest(
    string Title,
    decimal Amount,
    string Category,
    DateOnly ExpenseDate,
    string? Notes
);
