using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NftApi.Controllers;
using NftApi.Core.Data;
using NftApi.Core.Models;
using System.Dynamic;
using System.Text.Json;

namespace NftApi.Tests.Controllers;

public class SearchControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<SearchController>> _loggerMock;
    private readonly SearchController _controller;

    public SearchControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<SearchController>>();
        _controller = new SearchController(_context, _loggerMock.Object);

        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task SearchFinancialRecords_ValidName_ReturnsOpenRecords()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            Surname = "Smith",
            DateOfBirth = new DateTime(1980, 9, 23),
            Postcode = "CB3 5RR"
        };

        _context.People.Add(person);
        await _context.SaveChangesAsync();

        var record = new FinancialRecord
        {
            PersonId = person.Id,
            AccountType = "Mortgage",
            InitialAmount = 190000,
            RemainingAmount = 185320,
            TransactionDate = new DateTime(2022, 1, 12),
            Status = "OK"
        };

        _context.FinancialRecords.Add(record);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.SearchFinancialRecords("John Smith");

        // Assert - FIXED: Use dynamic or check the actual type
        var okResult = Assert.IsType<OkObjectResult>(result);

        // Use reflection to check if it's a list and get count
        var records = okResult.Value;
        Assert.NotNull(records);

        var recordList = records as System.Collections.IEnumerable;
        Assert.NotNull(recordList);

        int count = 0;
        foreach (var item in recordList) { count++; }
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task SearchFinancialRecords_InvalidName_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.SearchFinancialRecords("John");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Please provide both first name and surname (e.g., 'John Smith')", badRequestResult.Value);
    }

    [Fact]
    public async Task SearchFinancialRecords_EmptyName_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.SearchFinancialRecords("");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Name parameter is required", badRequestResult.Value);
    }

    [Fact]
    public async Task SearchFinancialRecords_NoMatchingPerson_ReturnsEmptyList()
    {
        // Act
        var result = await _controller.SearchFinancialRecords("Nonexistent Person");

        // Assert - FIXED: Use IEnumerable approach
        var okResult = Assert.IsType<OkObjectResult>(result);
        var records = okResult.Value as System.Collections.IEnumerable;
        Assert.NotNull(records);

        int count = 0;
        foreach (var item in records) { count++; }
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task SearchFinancialRecords_ClosedRecords_ReturnsEmptyList()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            Surname = "Smith",
            DateOfBirth = new DateTime(1980, 9, 23)
        };

        _context.People.Add(person);
        await _context.SaveChangesAsync();

        var closedRecord = new FinancialRecord
        {
            PersonId = person.Id,
            AccountType = "Loan",
            InitialAmount = 10000,
            RemainingAmount = 0, // Closed record
            TransactionDate = new DateTime(2022, 1, 12),
            Status = "Closed"
        };

        _context.FinancialRecords.Add(closedRecord);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.SearchFinancialRecords("John Smith");

        // Assert - FIXED: Use IEnumerable approach
        var okResult = Assert.IsType<OkObjectResult>(result);
        var records = okResult.Value as System.Collections.IEnumerable;
        Assert.NotNull(records);

        int count = 0;
        foreach (var item in records) { count++; }
        Assert.Equal(0, count);
    }

    public void Dispose()
    {
        _context?.Database?.EnsureDeleted();
        _context?.Dispose();
    }
}