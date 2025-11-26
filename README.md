# NFT API

A .NET 8 Web API for processing NFT-related financial data with JWT authentication.

## Features

- **JWT Authentication**: Secure API access with token-based authentication
- Upload and process CSV files containing people and financial records data
- Search for open financial records by exact name
- Comprehensive error handling and validation
- SQL Server database integration
- Structured logging and error responses

## Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB, Express, or full version)

## Setup

1. Clone the repository
2. Update connection string in `appsettings.json` if needed
3. **IMPORTANT**: Change the JWT SecretKey in `appsettings.json` for production use
4. Run database migrations:
   ```bash
   cd NftApi
   dotnet ef database update
   ```

## Running the Application

```bash
cd NftApi
dotnet run
```

The API will be available at `https://localhost:5279`

## Authentication

### Getting Started with JWT

All API endpoints (except authentication endpoints) require a valid JWT token.

#### 1. Register a New User

```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "your_username",
  "email": "your_email@example.com",
  "password": "YourSecurePassword123"
}
```

**Response:**

```json
{
  "success": true,
  "message": "Registration successful",
  "token": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "tokenType": "Bearer",
    "expiresIn": 3600,
    "issuedAt": "2024-11-26T13:02:29.000Z"
  }
}
```

#### 2. Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "your_username",
  "password": "YourSecurePassword123"
}
```

**Response:** (Same as register)

```json
{
  "success": true,
  "message": "Login successful",
  "token": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "tokenType": "Bearer",
    "expiresIn": 3600,
    "issuedAt": "2024-11-26T13:02:29.000Z"
  }
}
```

#### 3. Using the Token

Include the token in the `Authorization` header for all authenticated requests:

```http
Authorization: Bearer {your_access_token}
```

### Example: Search with Authentication

```http
GET /api/search/financial-records?name=John%20Smith
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## API Endpoints

### Authentication Endpoints (No token required)

- **POST** `/api/auth/register` - Register a new user
- **POST** `/api/auth/login` - Login and get JWT token

### Protected Endpoints (JWT token required)

- **GET** `/api/search/financial-records?name={name}` - Search financial records
- **POST** `/api/upload/people` - Upload people CSV file
- **POST** `/api/upload/financial-records` - Upload financial records CSV file

## Security Features

- **JWT Bearer Tokens** with configurable expiration (default: 60 minutes)
- **Password Hashing** using BCrypt with workfactor 12
- **Input Validation** on all endpoints
- **Unique Constraints** on username and email
- **Structured Error Responses** preventing information leakage
- **CORS** enabled for cross-origin requests

## Configuration

Update `appsettings.json` to customize:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=NftApiDb;..."
  },
  "Jwt": {
    "SecretKey": "your-super-secret-key-min-32-characters-change-in-production",
    "Issuer": "NftApi",
    "Audience": "NftApiUsers",
    "ExpirationMinutes": 60
  }
}
```

## Testing with REST Client

Use the provided `NftApi.http` file in VS Code with the REST Client extension:

1. Register a user
2. Copy the token from the response
3. Use `@authToken` variable in subsequent requests

## Database Schema

The application uses Entity Framework Core with SQL Server:

- **Users** - Authentication and user information
- **People** - Customer information
- **FinancialRecords** - Financial transaction records

## Troubleshooting

### Token Expired

If you get a 401 Unauthorized error, your token has expired. Register or login again to get a new token.

### Invalid Credentials

Ensure username and password match exactly. Usernames are case-sensitive.

### Connection String Error

Update the `DefaultConnection` in `appsettings.json` to match your SQL Server instance.

## Development

Run tests:

```bash
dotnet test
```

Build release version:

```bash
dotnet build -c Release
```

## License

MIT License
