namespace NftApi.Core.Models;

/// <summary>
/// Standard error response format for API
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// User-friendly error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Error code for identifying specific error types
    /// </summary>
    public string ErrorCode { get; set; } = "UNKNOWN_ERROR";

    /// <summary>
    /// Request ID for tracking/logging
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Additional details about the error
    /// </summary>
    public Dictionary<string, object>? Details { get; set; }
}
