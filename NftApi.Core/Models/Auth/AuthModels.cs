namespace NftApi.Core.Models.Auth;

/// <summary>
/// Request model for user login
/// </summary>
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Response model containing JWT token after successful login
/// </summary>
public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public string TokenType { get; set; } = "Bearer";

    public int ExpiresIn { get; set; }

    public DateTime IssuedAt { get; set; }
}

/// <summary>
/// Response model for registration
/// </summary>
public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Response model for successful operations
/// </summary>
public class AuthResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public TokenResponse? Token { get; set; }
}
