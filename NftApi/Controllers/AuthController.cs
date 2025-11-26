using Microsoft.AspNetCore.Mvc;
using NftApi.Core.Data;
using NftApi.Core.Models;
using NftApi.Core.Models.Auth;
using NftApi.Core.Services;

namespace NftApi.Controllers;

/// <summary>
/// Authentication controller for user login and registration
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ApplicationDbContext context,
        IAuthenticationService authService,
        ILogger<AuthController> logger)
    {
        _context = context;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// User login endpoint - returns JWT token upon successful authentication
    /// </summary>
    /// <param name="request">Login credentials (username and password)</param>
    /// <returns>JWT token if successful</returns>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Input validation
            if (request == null)
            {
                _logger.LogWarning("Login attempt with null request");
                return BadRequest(new ErrorResponse
                {
                    Message = "Login request cannot be null",
                    ErrorCode = "INVALID_REQUEST"
                });
            }

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Login attempt with missing credentials");
                return BadRequest(new ErrorResponse
                {
                    Message = "Username and password are required",
                    ErrorCode = "MISSING_CREDENTIALS"
                });
            }

            // Find user by username
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username && u.IsActive);

            if (user == null)
            {
                _logger.LogWarning("Login attempt for non-existent user: {Username}", request.Username);
                return Unauthorized(new ErrorResponse
                {
                    Message = "Invalid username or password",
                    ErrorCode = "INVALID_CREDENTIALS"
                });
            }

            // Verify password
            var passwordValid = await _authService.VerifyPasswordAsync(request.Password, user.PasswordHash);

            if (!passwordValid)
            {
                _logger.LogWarning("Failed login attempt for user: {Username}", user.Username);
                return Unauthorized(new ErrorResponse
                {
                    Message = "Invalid username or password",
                    ErrorCode = "INVALID_CREDENTIALS"
                });
            }

            // Generate token
            var tokenResponse = _authService.GenerateToken(user);

            _logger.LogInformation("User {Username} successfully logged in", user.Username);

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Login successful",
                Token = tokenResponse
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Message = "An error occurred during login",
                ErrorCode = "LOGIN_ERROR"
            });
        }
    }

    /// <summary>
    /// User registration endpoint - creates new user account
    /// </summary>
    /// <param name="request">Registration details (username, email, password)</param>
    /// <returns>JWT token if successful</returns>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Input validation
            if (request == null)
            {
                _logger.LogWarning("Registration attempt with null request");
                return BadRequest(new ErrorResponse
                {
                    Message = "Registration request cannot be null",
                    ErrorCode = "INVALID_REQUEST"
                });
            }

            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Registration attempt with missing fields");
                return BadRequest(new ErrorResponse
                {
                    Message = "Username, email, and password are required",
                    ErrorCode = "MISSING_FIELDS"
                });
            }

            // Validate password strength (minimum 8 characters)
            if (request.Password.Length < 8)
            {
                return BadRequest(new ErrorResponse
                {
                    Message = "Password must be at least 8 characters long",
                    ErrorCode = "WEAK_PASSWORD"
                });
            }

            // Check if username already exists
            if (_context.Users.Any(u => u.Username == request.Username))
            {
                _logger.LogWarning("Registration attempt with existing username: {Username}", request.Username);
                return Conflict(new ErrorResponse
                {
                    Message = "Username already exists",
                    ErrorCode = "USERNAME_EXISTS"
                });
            }

            // Check if email already exists
            if (_context.Users.Any(u => u.Email == request.Email))
            {
                _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
                return Conflict(new ErrorResponse
                {
                    Message = "Email already exists",
                    ErrorCode = "EMAIL_EXISTS"
                });
            }

            // Validate email format (basic check)
            if (!request.Email.Contains("@") || !request.Email.Contains("."))
            {
                return BadRequest(new ErrorResponse
                {
                    Message = "Invalid email format",
                    ErrorCode = "INVALID_EMAIL"
                });
            }

            // Create new user
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = _authService.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New user registered: {Username}", user.Username);

            // Generate token for new user
            var tokenResponse = _authService.GenerateToken(user);

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Registration successful",
                Token = tokenResponse
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Message = "An error occurred during registration",
                ErrorCode = "REGISTRATION_ERROR"
            });
        }
    }
}
