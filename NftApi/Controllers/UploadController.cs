using Microsoft.AspNetCore.Mvc;
using NftApi.Core.Services;

namespace NftApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IFileProcessingService _fileProcessingService;
    private readonly ILogger<UploadController> _logger;

    public UploadController(IFileProcessingService fileProcessingService, ILogger<UploadController> logger)
    {
        _fileProcessingService = fileProcessingService;
        _logger = logger;
    }

    [HttpPost("people")]
    public async Task<IActionResult> UploadPeopleFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        if (Path.GetExtension(file.FileName).ToLower() != ".csv")
        {
            return BadRequest("Only CSV files are allowed");
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _fileProcessingService.ProcessPeopleFileAsync(stream);

            var response = new
            {
                SuccessfulRecords = result.SuccessfulRecords,
                FailedRecords = result.FailedRecords,
                Errors = result.Errors
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading people file");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("financial-records")]
    public async Task<IActionResult> UploadFinancialRecordsFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        if (Path.GetExtension(file.FileName).ToLower() != ".csv")
        {
            return BadRequest("Only CSV files are allowed");
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _fileProcessingService.ProcessFinancialRecordsFileAsync(stream);

            var response = new
            {
                SuccessfulRecords = result.SuccessfulRecords,
                FailedRecords = result.FailedRecords,
                Errors = result.Errors
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading financial records file");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}