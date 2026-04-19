namespace ExpenseTracker.Api.Models;

public record ExpenseDto(
    int Id,
    string Title,
    decimal Amount,
    string Category,
    DateOnly ExpenseDate,
    string? Notes,
    DateTime CreatedAtUtc
);
