# Training Management System (TMS) — Backend
# TMS Backend

ASP.NET Core Web API backend for a Training Management System (TMS). It provides authentication, course management, student enrollment, assessments, certificates, transcripts, notifications, and administrative features.

## Tech Stack

- ASP.NET Core
- C#
- Entity Framework Core
- PostgreSQL
- ASP.NET Core Identity
- JWT Bearer Authentication
- SignalR
- OpenAPI / Scalar
- API Versioning
- xUnit + NSubstitute

## Architecture

The backend follows a layered architecture with separation of concerns.

```text
TmsApi/
│
├── TmsApi.Api/
│   ├── Controllers / API endpoints
│   ├── Authentication & authorization
│   ├── Middleware
│   └── Application startup
│
├── TmsApi.Application/
│   ├── Business logic
│   ├── Application services
│   ├── DTOs
│   └── Contracts / interfaces
│
├── TmsApi.Domain/
│   ├── Entities
│   ├── Domain models
│   └── Business rules
│
├── TmsApi.Infrastructure/
│   ├── EF Core & PostgreSQL
│   ├── Identity
│   ├── Data seeding
│   ├── Repositories
│   ├── SignalR
│   └── Background workers
│
├── TmsApi.Tests/
│   └── Automated tests
│
├── docs/
│   └── Project documentation
│
├── TmsApi.http
└── TmsApi.slnx
```

## Key Features

- User registration and login
- JWT access and refresh tokens
- Role-based authorization
- Course management
- Instructor ownership and resource authorization
- Student enrollment and approval
- Assessments and grading
- Certificates and transcripts
- Real-time notifications with SignalR
- Rate limiting
- Security headers
- API versioning
- OpenAPI documentation
- Automated backend tests

## Prerequisites

- .NET SDK
- PostgreSQL
- Entity Framework Core CLI

## Getting Started

Clone the repository:

```bash
git clone https://github.com/Ebropro/tms-Backend.git
cd tms-Backend
```

### Configure the Database

Create:

```text
TmsApi.Api/appsettings.Development.json
```

Example:

```json
{
  "ConnectionStrings": {
    "TmsDatabase": "Host=localhost;Database=TmsDb;Username=YOUR_POSTGRES_USERNAME;Password=YOUR_POSTGRES_PASSWORD"
  }
}
```

Use your own PostgreSQL credentials.

**Do not commit `appsettings.Development.json` or database passwords.**

### Apply Migrations

From the backend root:

```bash
dotnet ef database update --project TmsApi.Infrastructure --startup-project TmsApi.Api
```

The application includes seed data for development and testing.

### Run the API

```bash
dotnet run --project TmsApi.Api
```

The API provides OpenAPI documentation and Scalar for interactive API testing.

## Authentication

Protected endpoints require a JWT bearer token:

```text
Authorization: Bearer <access-token>
```

The API includes role-based and resource-based authorization to protect restricted operations.

## API Documentation

The API supports versioned documentation, including:

```text
/openapi/v1.json
/openapi/v2.json
```

Scalar provides an interactive interface for exploring and testing the API.

## Security

Security features include:

- JWT authentication
- ASP.NET Core Identity
- Account lockout
- Role-based authorization
- Resource-based authorization
- Login rate limiting
- Security response headers
- Protected development configuration

## Testing

Run all backend tests with:

```bash
dotnet test
```

HTTP requests can also be tested using:

```text
TmsApi.http
```

## Frontend

The Angular frontend is maintained in a separate repository:

```text
tms-Frontend
```

The frontend communicates with this ASP.NET Core API.

## Project Structure

```text
TMS
├── tms-Backend   → ASP.NET Core API
└── tms-Frontend  → Angular application
```

## Development

Useful commands:

```bash
dotnet build
dotnet test
dotnet run --project TmsApi.Api
dotnet ef database update --project TmsApi.Infrastructure --startup-project TmsApi.Api
```

Development configuration files containing local credentials are excluded from Git.

## License

This project was developed as a Training Management System full-stack application.