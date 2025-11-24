using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NftApi.Core.Data;
using NftApi.Core.Models;
using NftApi.Core.Services;
using System.Text;

namespace NftApi.Tests.Services;

public class FileProcessingServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<FileProcessingService>> _loggerMock;
    private readonly FileProcessingService _service;

    public FileProcessingServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<FileProcessingService>>();
        _service = new FileProcessingService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessPeopleFileAsync_ValidCsv_ReturnsSuccessResult()
    {
        // Arrange
        var csvContent = "FirstName,Surname,Dob,Address,Postcode\nJohn,Smith,23/09/1980,15 Station Road,CB3 5RR";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act
        var result = await _service.ProcessPeopleFileAsync(stream);

        // Assert
        Assert.Equal(1, result.SuccessfulRecords);
        Assert.Equal(0, result.FailedRecords);
        Assert.Empty(result.Errors);
        Assert.Single(await _context.People.ToListAsync());
    }

    [Fact]
    public async Task ProcessPeopleFileAsync_InvalidCsv_ReturnsErrorResult()
    {
        // Arrange
        var csvContent = "FirstName,Surname,Dob,Address,Postcode\nJohn,,23/09/1980,15 Station Road,CB3 5RR";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act
        var result = await _service.ProcessPeopleFileAsync(stream);

        // Assert
        Assert.Equal(0, result.SuccessfulRecords);
        Assert.Equal(1, result.FailedRecords);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task ProcessFinancialRecordsFileAsync_ValidCsv_ReturnsSuccessResult()
    {
        // Arrange
        // First, add a person to match with
        var person = new Person
        {
            FirstName = "John",
            Surname = "Smith",
            DateOfBirth = new DateTime(1980, 9, 23),
            Postcode = "CB3 5RR"
        };
        await _context.People.AddAsync(person);
        await _context.SaveChangesAsync();

        var csvContent = "FirstName,Surname,Dob,Postcode,AccountType,InitialAmount,TotalPaymentAmount,RepaymentAmount,RemainingAmount,TransactionDate,MinimumPaymentAmount,InterestRate,InitialTerm,RemainingTerm,Status\nJohn,Smith,23/09/1980,CB3 5RR,Mortgage,190000,,,,12/07/2021,960,3.1,240,240,InitialPurchase";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act
        var result = await _service.ProcessFinancialRecordsFileAsync(stream);

        // Assert
        Assert.Equal(1, result.SuccessfulRecords);
        Assert.Equal(0, result.FailedRecords);
        Assert.Empty(result.Errors);
        Assert.Single(await _context.FinancialRecords.ToListAsync());
    }
}