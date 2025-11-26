using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NftApi.Core.Models;
using NftApi.Core.Models.Auth;

namespace NftApi.Core.Services;

/// <summary>
/// Service for JWT token generation and validation
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Generates a JWT token for authenticated user
    /// </summary>
    TokenResponse GenerateToken(User user);

    /// <summary>
    /// Verifies user credentials
    /// </summary>
    Task<bool> VerifyPasswordAsync(string password, string hash);

    /// <summary>
    /// Hashes a password using bcrypt
    /// </summary>
    string HashPassword(string password);
}

public class JwtAuthenticationService : IAuthenticationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtAuthenticationService> _logger;

    public JwtAuthenticationService(IConfiguration configuration, ILogger<JwtAuthenticationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public TokenResponse GenerateToken(User user)
    {
        try
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("IsActive", user.IsActive.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation("JWT token generated for user {Username}", user.Username);

            return new TokenResponse
            {
                AccessToken = tokenString,
                TokenType = "Bearer",
                ExpiresIn = expirationMinutes * 60,
                IssuedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating JWT token for user {UserId}", user.Id);
            throw;
        }
    }

    public Task<bool> VerifyPasswordAsync(string password, string hash)
    {
        try
        {
            // Using bcrypt for password verification
            bool isValid = BCrypt.Net.BCrypt.Verify(password, hash);
            return Task.FromResult(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying password");
            return Task.FromResult(false);
        }
    }

    public string HashPassword(string password)
    {
        try
        {
            // Using bcrypt for password hashing (cost factor 12 for security)
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hashing password");
            throw;
        }
    }
}
