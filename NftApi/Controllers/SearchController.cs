using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NftApi.Core.Data;
using NftApi.Core.Models;

namespace NftApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SearchController> _logger;

    public SearchController(ApplicationDbContext context, ILogger<SearchController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("financial-records")]
    public async Task<IActionResult> SearchFinancialRecords([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Name parameter is required");
        }

        try
        {
            var names = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (names.Length != 2)
            {
                return BadRequest("Please provide both first name and surname (e.g., 'John Smith')");
            }

            var firstName = names[0].Trim();
            var surname = names[1].Trim();
            var today = DateTime.UtcNow.Date;

            var records = await _context.FinancialRecords
                .Include(fr => fr.Person)
                .Where(fr => fr.Person.FirstName == firstName &&
                            fr.Person.Surname == surname &&
                            fr.TransactionDate <= today &&
                            fr.RemainingAmount > 0 &&
                            fr.Status != "Closed")
                .OrderByDescending(fr => fr.TransactionDate)
                .Select(fr => new
                {
                    fr.Id,
                    Person = $"{fr.Person.FirstName} {fr.Person.Surname}",
                    fr.AccountType,
                    fr.InitialAmount,
                    fr.RemainingAmount,
                    fr.TransactionDate,
                    fr.Status,
                    fr.InterestRate,
                    IsOpen = fr.RemainingAmount > 0 && fr.Status != "Closed"
                })
                .ToListAsync();

            return Ok(records); // This returns OkObjectResult
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching financial records for name {Name}", name);
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}