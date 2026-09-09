# Training Management System (TMS) — Backend

A secure, modular REST API for a Training Management System (TMS), built with ASP.NET Core and designed to work with the Angular TMS frontend.

The backend provides authentication, authorization, course management, student enrollment, assessments, grading, certificates, transcripts, notifications, and related training-management functionality.

## Technology Stack

* **ASP.NET Core**
* **C#**
* **Entity Framework Core**
* **PostgreSQL**
* **ASP.NET Core Identity**
* **JWT Bearer Authentication**
* **Role-based and resource-based authorization**
* **SignalR**
* **OpenAPI**
* **Scalar API Reference**
* **API Versioning**
* **xUnit / .NET testing**

## Architecture

The backend follows a layered architecture:


TmsApi
│
├── TmsApi.Api
│   ├── API endpoints/controllers
│   ├── Authentication and authorization configuration
│   ├── Middleware
│   ├── OpenAPI configuration
│   └── Application startup
│
├── TmsApi.Application
│   ├── Application services
│   ├── Business logic
│   ├── DTOs
│   └── Application contracts
│
├── TmsApi.Domain
│   ├── Domain entities
│   ├── Domain models
│   └── Core business rules
│
├── TmsApi.Infrastructure
│   ├── Entity Framework Core
│   ├── PostgreSQL configuration
│   ├── Identity
│   ├── Data seeding
│   ├── Repositories/infrastructure services
│   ├── SignalR
│   └── Background workers
│
├── TmsApi.Tests
│   └── Automated backend tests
│
├── docs
│   └── Project/API documentation
│
├── TmsApi.http
│   └── HTTP request examples
│
└── TmsApi.slnx
    └── Solution file
```

## Main Features

The API supports functionality including:

* User registration and authentication
* JWT access tokens
* Refresh tokens
* ASP.NET Core Identity
* Role-based authorization
* Course management
* Instructor ownership/resource authorization
* Student enrollment
* Enrollment approval workflows
* Assessments and grading
* Certificates
* Student transcripts
* Notifications
* Real-time updates through SignalR
* API rate limiting
* Security response headers
* API versioning
* OpenAPI documentation
* Automated tests

## Prerequisites

Before running the backend, install:

1. **.NET SDK** compatible with the project
2. **PostgreSQL**
3. **Git**

You should also have access to the Angular frontend if you want to run the complete TMS application.

## Getting the Project

Clone the backend repository:


git clone https://github.com/Ebropro/tms-Backend.git
cd tms-Backend


## Database Configuration

The application uses PostgreSQL.

The repository intentionally does **not** contain the developer-specific database configuration file because connection strings can contain passwords and other sensitive information.

A safe configuration template is provided as:


appsettings.Development.example.json


### Configure your local database

1. Install and start PostgreSQL.
2. Create a PostgreSQL database named:

TmsDb

3. Copy the example configuration:

### PowerShell

```
Copy-Item "TmsApi.Api\appsettings.Development.example.json" `
          "TmsApi.Api\appsettings.Development.json"
```

4. Open:

```text
TmsApi.Api\appsettings.Development.json
```

5. Replace the placeholder PostgreSQL username and password with your own local credentials.

For example:

```json
{
  "ConnectionStrings": {
    "TmsDatabase": "Host=localhost;Database=TmsDb;Username=postgres;Password=YOUR_PASSWORD"
  }
}


## Entity Framework Core Database

After configuring the database connection, apply the existing migrations:

```
dotnet ef database update
```

If the Entity Framework CLI is not installed:

```
dotnet tool install --global dotnet-ef
```

Depending on the project configuration, you may need to specify the API and infrastructure projects explicitly. If so, use the EF Core command appropriate to the solution's configured `DbContext` and migrations location.

## Seed Data

The application contains a data seeding mechanism for development/testing.

The seeded data includes examples of:

* Courses
* Students
* Assessments
* Certificates
* Instructor assignments

This allows the application to be started with representative training-management data rather than an empty database.

## Running the API

From the repository root:

```
dotnet run --project TmsApi.Api
```

The API will start using the configured ASP.NET Core launch settings.

The exact HTTP/HTTPS ports may vary depending on the local launch configuration.

## API Documentation

When the API is running, OpenAPI documentation is available through the configured API documentation endpoints.

The project uses **Scalar** as the interactive API reference.

Depending on the active launch configuration, open the Scalar interface in your browser using the API's configured Scalar URL.

The API supports versioned documentation, including:

```
v1
v2
```

API routes use versioned paths such as:

```
/api/v1/auth/login
/api/v2/courses
```

## Authentication

Protected endpoints use JWT Bearer authentication.

After obtaining an access token from the authentication endpoint, include it in requests using:

```
Authorization: Bearer YOUR_ACCESS_TOKEN
```

Authentication and authorization are handled by the ASP.NET Core Identity and JWT configuration.

## Authorization

The API uses multiple levels of authorization, including:

* Authentication requirements
* Role-based authorization
* Resource-based authorization

For example, users may be restricted from modifying resources owned by another instructor.

This helps prevent authenticated users from accessing or modifying resources they are not authorized to manage.

## Rate Limiting

The API includes rate-limiting protections for sensitive operations such as authentication.

Repeated requests beyond the configured limit can result in:

```
429 Too Many Requests
```

This helps protect authentication endpoints from excessive request attempts.

## Security

The backend includes security-related protections such as:

* JWT authentication
* ASP.NET Core Identity
* Account lockout
* Role-based authorization
* Resource-based authorization
* Rate limiting
* Security response headers
* Protected API endpoints
* Secure handling of development configuration
* Validation of authenticated requests

Development credentials and database passwords should never be committed to source control.

## Testing

The repository contains a dedicated test project:

```
TmsApi.Tests
```

Run the test suite with:

```bash
dotnet test
```

For a more verbose test run:

```bash
dotnet test --verbosity normal
```

## HTTP Requests

The repository contains:

```text
TmsApi.http
```

which can be used with compatible IDE tooling to manually test API endpoints.

For interactive API testing, Scalar/OpenAPI can also be used while the API is running.

## Project Relationship

This repository contains the **backend** portion of the Training Management System.

The Angular frontend is maintained separately in:

tms-frontend

The two applications communicate through the HTTP API.

Conceptually:


┌─────────────────────────┐
│     Angular Frontend    │
│       tms-frontend      │
└────────────┬────────────┘
             │
             │ HTTP / HTTPS
             │ JWT Bearer
             ▼
┌─────────────────────────┐
│     ASP.NET Core API    │
│       TMS-Backend       │
└────────────┬────────────┘
             │
             │ Entity Framework Core
             ▼
┌─────────────────────────┐
│       PostgreSQL        │
│          TmsDb          │
└─────────────────────────┘
```

## Development Configuration

The following file is intentionally excluded from Git:

TmsApi.Api/appsettings.Development.json


## Useful Commands

Restore dependencies:

```bash
dotnet restore
```

Build the solution:

```bash
dotnet build
```

Run the API:

```bash
dotnet run --project TmsApi.Api
```

Run tests:

```bash
dotnet test
```

Apply EF Core migrations:

```bash
dotnet ef database update
```

Check Git status:

```bash
git status
```

## Repository Structure

The backend repository intentionally contains source code, configuration templates, documentation, tests, and project files required to build and run the API.

Generated files such as:

```text
bin/
obj/
.vs/
publish/
```

and local development secrets are excluded through `.gitignore`.

## License

This project was developed as a Training Management System application as part of a full-stack software development project program.
