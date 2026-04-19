using System.Net;
using System.Net.Http.Json;
using ExpenseTracker.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ExpenseTracker.Tests;

public class ExpensesApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ExpensesApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(_ =>
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Data Source=expense-tracker-test.db");
        });
    }

    [Fact]
    public async Task GetSummary_ReturnsZeroTotals_WhenNoExpensesExist()
    {
        var client = _factory.CreateClient();

        var summary = await client.GetFromJsonAsync<ExpenseSummaryDto>("/api/expenses/summary");

        Assert.NotNull(summary);
        Assert.Equal(0, summary!.Count);
        Assert.Equal(0m, summary.TotalAmount);
        Assert.Empty(summary.Breakdown);
    }

    [Fact]
    public async Task PostExpense_ThenGetSummary_ReturnsAggregatedTotals()
    {
        var client = _factory.CreateClient();

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
}
