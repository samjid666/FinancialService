namespace NftApi.Core.Models;

/// <summary>
/// Represents a user in the system for authentication.
/// </summary>
public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password using bcrypt
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}
