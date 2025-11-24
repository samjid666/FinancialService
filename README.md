# NFT API

A .NET 8 Web API for processing NFT-related financial data.

## Features

- Upload and process CSV files containing people and financial records data
- Search for open financial records by exact name
- Comprehensive error handling and validation
- SQL Server database integration

## Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB, Express, or full version)

## Setup

1. Clone the repository
2. Update connection string in `appsettings.json` if needed
3. Run database migrations:
   ```bash
   cd NftApi
   dotnet ef database update
   ```
