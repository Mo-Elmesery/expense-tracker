using System.Net;
using System.Net.Http.Json;
using ExpenseTracker.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ExpenseTracker.Tests;

public class ExpensesApiTests
{
    private readonly WebApplicationFactory<Program> _factory;

    public ExpensesApiTests()
    {
        _factory = new WebApplicationFactory<Program>();
    }

    [Fact]
    public async Task GetSummary_ReturnsZeroTotals_WhenNoExpensesExist()
    {
        var client = CreateClientWithUniqueDb();

        var summary = await client.GetFromJsonAsync<ExpenseSummaryDto>("/api/expenses/summary");

        Assert.NotNull(summary);
        Assert.Equal(0, summary!.Count);
        Assert.Equal(0m, summary.TotalAmount);
        Assert.Empty(summary.Breakdown);
    }

    [Fact]
    public async Task PostExpense_ThenGetSummary_ReturnsAggregatedTotals()
    {
        var client = CreateClientWithUniqueDb();

        var expense = new CreateExpenseRequest(
            "مواصلات",
            150.75m,
            "النقل",
            new DateOnly(2026, 4, 19),
            "تاكسي"
        );

        var postResponse = await client.PostAsJsonAsync("/api/expenses", expense);

        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var summary = await client.GetFromJsonAsync<ExpenseSummaryDto>("/api/expenses/summary");
        var expenses = await client.GetFromJsonAsync<List<ExpenseDto>>("/api/expenses");

        Assert.NotNull(summary);
        Assert.Equal(1, summary!.Count);
        Assert.Equal(150.75m, summary.TotalAmount);
        Assert.Single(summary.Breakdown);
        Assert.Equal("النقل", summary.Breakdown[0].Category);
        Assert.Equal(150.75m, summary.Breakdown[0].TotalAmount);

        Assert.NotNull(expenses);
        Assert.Single(expenses!);
        Assert.Equal("مواصلات", expenses[0].Title);
        Assert.Equal("تاكسي", expenses[0].Notes);
    }

    [Fact]
    public async Task PutExpense_UpdateExistingExpense_ReturnsUpdatedDto()
    {
        var client = CreateClientWithUniqueDb();

        // Create an expense first
        var createExpense = new CreateExpenseRequest(
            "مواصلات أصلية",
            150.75m,
            "النقل",
            new DateOnly(2026, 4, 19),
            "تاكسي أصلي"
        );

        var postResponse = await client.PostAsJsonAsync("/api/expenses", createExpense);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var createdExpenses = await client.GetFromJsonAsync<List<ExpenseDto>>("/api/expenses");
        var id = createdExpenses![0].Id;

        // Update the expense
        var updateExpense = new UpdateExpenseRequest(
            "مواصلات محدثة",
            200.50m,
            "مواصلات",
            new DateOnly(2026, 4, 20),
            "ملاحظات محدثة"
        );

        var putResponse = await client.PutAsJsonAsync($"/api/expenses/{id}", updateExpense);

        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var updatedExpense = await client.GetFromJsonAsync<ExpenseDto>($"/api/expenses/{id}");

        Assert.NotNull(updatedExpense);
        Assert.Equal("مواصلات محدثة", updatedExpense!.Title);
        Assert.Equal(200.50m, updatedExpense.Amount);
        Assert.Equal("مواصلات", updatedExpense.Category);
        Assert.Equal("2026-04-20", updatedExpense.ExpenseDate.ToString("yyyy-MM-dd"));
        Assert.Equal("ملاحظات محدثة", updatedExpense.Notes);
    }

    [Fact]
    public async Task PutExpense_NonexistentExpense_ReturnsNotFound()
    {
        var client = CreateClientWithUniqueDb();

        var updateExpense = new UpdateExpenseRequest(
            "محاولات تحديث",
            100m,
            "تصنيف",
            new DateOnly(2026, 4, 20),
            "ملاحظات"
        );

        var putResponse = await client.PutAsJsonAsync("/api/expenses/9999", updateExpense);

        Assert.Equal(HttpStatusCode.NotFound, putResponse.StatusCode);
    }

    private HttpClient CreateClientWithUniqueDb()
    {
        var tempDbPath = Path.Combine(Path.GetTempPath(), $"expense-tracker-test-{Guid.NewGuid():N}.db");
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", $"Data Source={tempDbPath}");
        return _factory.CreateClient();
    }
}
