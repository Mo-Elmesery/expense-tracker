using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Mappers;
using ExpenseTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly ExpenseTrackerDbContext _dbContext;

    public ExpensesController(ExpenseTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExpenseDto>>> GetAll([FromQuery] string? category = null)
    {
        var query = _dbContext.Expenses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(x => x.Category == category);
        }

        var expenses = await query
            .OrderByDescending(x => x.ExpenseDate)
            .ThenByDescending(x => x.Id)
            .Select(x => x.ToDto())
            .ToListAsync();

        return Ok(expenses);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExpenseDto>> Get(int id)
    {
        var expense = await _dbContext.Expenses.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => x.ToDto())
            .FirstOrDefaultAsync();

        if (expense is null)
        {
            return NotFound();
        }

        return Ok(expense);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ExpenseSummaryDto>> GetSummary()
    {
        var items = await _dbContext.Expenses.AsNoTracking().ToListAsync();

        var breakdown = items
            .GroupBy(x => x.Category)
            .Select(group => new CategorySummaryDto(
                group.Key,
                group.Sum(x => x.Amount),
                group.Count()))
            .OrderByDescending(x => x.TotalAmount)
            .ToList();

        var summary = new ExpenseSummaryDto(
            items.Sum(x => x.Amount),
            items.Count,
            breakdown);

        return Ok(summary);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create([FromBody] CreateExpenseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            ModelState.AddModelError(nameof(request.Title), "Title is required.");
            return ValidationProblem(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.Category))
        {
            ModelState.AddModelError(nameof(request.Category), "Category is required.");
            return ValidationProblem(ModelState);
        }

        if (request.Amount <= 0)
        {
            ModelState.AddModelError(nameof(request.Amount), "Amount must be greater than zero.");
            return ValidationProblem(ModelState);
        }

        var expense = new Expense
        {
            Title = request.Title.Trim(),
            Amount = decimal.Round(request.Amount, 2),
            Category = request.Category.Trim(),
            ExpenseDate = request.ExpenseDate,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Expenses.Add(expense);
        await _dbContext.SaveChangesAsync();

        var dto = expense.ToDto();
        return CreatedAtAction(nameof(GetAll), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ExpenseDto>> Update(int id, [FromBody] UpdateExpenseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            ModelState.AddModelError(nameof(request.Title), "Title is required.");
            return ValidationProblem(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.Category))
        {
            ModelState.AddModelError(nameof(request.Category), "Category is required.");
            return ValidationProblem(ModelState);
        }

        if (request.Amount <= 0)
        {
            ModelState.AddModelError(nameof(request.Amount), "Amount must be greater than zero.");
            return ValidationProblem(ModelState);
        }

        var expense = await _dbContext.Expenses.FindAsync(id);
        if (expense is null)
        {
            return NotFound();
        }

        expense.Title = request.Title.Trim();
        expense.Amount = decimal.Round(request.Amount, 2);
        expense.Category = request.Category.Trim();
        expense.ExpenseDate = request.ExpenseDate;
        expense.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await _dbContext.SaveChangesAsync();

        var dto = expense.ToDto();
        return Ok(dto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var expense = await _dbContext.Expenses.FindAsync(id);
        if (expense is null)
        {
            return NotFound();
        }

        _dbContext.Expenses.Remove(expense);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
