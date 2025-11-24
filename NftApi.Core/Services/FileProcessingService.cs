using CsvHelper;
using Microsoft.Extensions.Logging;
using NftApi.Core.Data;
using NftApi.Core.Models;
using NftApi.Core.Models.Csv;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NftApi.Core.Services
{
    public class FileProcessingService : IFileProcessingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FileProcessingService> _logger;

        public FileProcessingService(ApplicationDbContext context, ILogger<FileProcessingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ProcessingResult> ProcessPeopleFileAsync(Stream fileStream)
        {
            var errors = new List<string>();
            int successfulRecords = 0;
            int failedRecords = 0;

            try
            {
                using var reader = new StreamReader(fileStream);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                csv.Context.RegisterClassMap<PersonCsvMap>();

                var records = csv.GetRecords<PersonCsv>().ToList();
                var people = new List<Person>();

                for (int i = 0; i < records.Count; i++)
                {
                    try
                    {
                        var record = records[i];
                        var validationResult = ValidatePersonRecord(record, i + 2); // +2 for header row and 1-based index

                        if (!validationResult.IsValid)
                        {
                            failedRecords++;
                            errors.AddRange(validationResult.Errors);
                            continue;
                        }

                        var person = new Person
                        {
                            FirstName = record.FirstName!.Trim(),
                            Surname = record.Surname!.Trim(),
                            DateOfBirth = ParseDate(record.Dob!),
                            Address = record.Address?.Trim(),
                            Postcode = record.Postcode?.Trim()
                        };

                        // Check for duplicates
                        var exists = await _context.People.AnyAsync(p =>
                            p.FirstName == person.FirstName &&
                            p.Surname == person.Surname &&
                            p.DateOfBirth == person.DateOfBirth);

                        if (!exists)
                        {
                            people.Add(person);
                        }

                        successfulRecords++;
                    }
                    catch (Exception ex)
                    {
                        failedRecords++;
                        errors.Add($"Row {i + 2}: Unexpected error - {ex.Message}");
                        _logger.LogError(ex, "Error processing person record at row {RowNumber}", i + 2);
                    }
                }

                if (people.Any())
                {
                    await _context.People.AddRangeAsync(people);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                errors.Add($"File processing failed: {ex.Message}");
                _logger.LogError(ex, "Error processing people file");
                return new ProcessingResult(0, 0, errors);
            }

            return new ProcessingResult(successfulRecords, failedRecords, errors);
        }

        public async Task<ProcessingResult> ProcessFinancialRecordsFileAsync(Stream fileStream)
        {
            var errors = new List<string>();
            int successfulRecords = 0;
            int failedRecords = 0;
            var financialRecords = new List<FinancialRecord>();

            try
            {
                using var reader = new StreamReader(fileStream);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                csv.Context.RegisterClassMap<FinancialRecordCsvMap>();

                var records = csv.GetRecords<FinancialRecordCsv>().ToList();

                for (int i = 0; i < records.Count; i++)
                {
                    try
                    {
                        var record = records[i];
                        var validationResult = ValidateFinancialRecord(record, i + 2);

                        if (!validationResult.IsValid)
                        {
                            failedRecords++;
                            errors.AddRange(validationResult.Errors);
                            continue;
                        }

                        // Find matching person
                        var person = await FindPersonAsync(record);
                        if (person == null)
                        {
                            failedRecords++;
                            errors.Add($"Row {i + 2}: No matching person found for {record.FirstName} {record.Surname}");
                            continue;
                        }

                        var financialRecord = new FinancialRecord
                        {
                            PersonId = person.Id,
                            AccountType = record.AccountType!.Trim(),
                            InitialAmount = decimal.Parse(record.InitialAmount!),
                            TotalPaymentAmount = ParseDecimal(record.TotalPaymentAmount),
                            RepaymentAmount = ParseDecimal(record.RepaymentAmount),
                            RemainingAmount = ParseDecimal(record.RemainingAmount),
                            TransactionDate = ParseDate(record.TransactionDate!),
                            MinimumPaymentAmount = ParseDecimal(record.MinimumPaymentAmount),
                            InterestRate = ParseDecimal(record.InterestRate),
                            InitialTerm = ParseInt(record.InitialTerm),
                            RemainingTerm = ParseInt(record.RemainingTerm),
                            Status = record.Status?.Trim() ?? "OK"
                        };

                        financialRecords.Add(financialRecord);
                        successfulRecords++;
                    }
                    catch (Exception ex)
                    {
                        failedRecords++;
                        errors.Add($"Row {i + 2}: Unexpected error - {ex.Message}");
                        _logger.LogError(ex, "Error processing financial record at row {RowNumber}", i + 2);
                    }
                }

                if (financialRecords.Any())
                {
                    await _context.FinancialRecords.AddRangeAsync(financialRecords);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                errors.Add($"File processing failed: {ex.Message}");
                _logger.LogError(ex, "Error processing financial records file");
                return new ProcessingResult(0, 0, errors);
            }

            return new ProcessingResult(successfulRecords, failedRecords, errors);
        }

        private (bool IsValid, List<string> Errors) ValidatePersonRecord(PersonCsv record, int rowNumber)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(record.FirstName))
                errors.Add($"Row {rowNumber}: FirstName is required");

            if (string.IsNullOrWhiteSpace(record.Surname))
                errors.Add($"Row {rowNumber}: Surname is required");

            if (string.IsNullOrWhiteSpace(record.Dob))
                errors.Add($"Row {rowNumber}: Date of birth is required");
            else if (!TryParseDate(record.Dob, out _))
                errors.Add($"Row {rowNumber}: Invalid date format for Dob");

            return (!errors.Any(), errors);
        }

        private (bool IsValid, List<string> Errors) ValidateFinancialRecord(FinancialRecordCsv record, int rowNumber)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(record.FirstName))
                errors.Add($"Row {rowNumber}: FirstName is required");

            if (string.IsNullOrWhiteSpace(record.Surname))
                errors.Add($"Row {rowNumber}: Surname is required");

            if (string.IsNullOrWhiteSpace(record.AccountType))
                errors.Add($"Row {rowNumber}: AccountType is required");

            if (string.IsNullOrWhiteSpace(record.InitialAmount) || !decimal.TryParse(record.InitialAmount, out _))
                errors.Add($"Row {rowNumber}: Valid InitialAmount is required");

            if (string.IsNullOrWhiteSpace(record.TransactionDate) || !TryParseDate(record.TransactionDate, out _))
                errors.Add($"Row {rowNumber}: Valid TransactionDate is required");

            return (!errors.Any(), errors);
        }

        private async Task<Person?> FindPersonAsync(FinancialRecordCsv record)
        {
            if (!TryParseDate(record.Dob, out var dob))
                return null;

            return await _context.People.FirstOrDefaultAsync(p =>
                p.FirstName == record.FirstName!.Trim() &&
                p.Surname == record.Surname!.Trim() &&
                p.DateOfBirth == dob);
        }

        private DateTime ParseDate(string dateString)
        {
            var formats = new[] { "dd/MM/yyyy", "d/M/yyyy", "d/MM/yyyy", "MM/dd/yyyy", "M/d/yyyy", "yyyy-MM-dd" };

            if (DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                return result.Date;

            throw new FormatException($"Invalid date format: {dateString}");
        }

        private bool TryParseDate(string? dateString, out DateTime result)
        {
            result = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(dateString)) return false;

            var formats = new[] { "dd/MM/yyyy", "d/M/yyyy", "d/MM/yyyy", "MM/dd/yyyy", "M/d/yyyy", "yyyy-MM-dd" };
            return DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        }

        private decimal? ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return decimal.TryParse(value, out var result) ? result : null;
        }

        private int? ParseInt(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return int.TryParse(value, out var result) ? result : null;
        }
    }
}
