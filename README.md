# .NET 9 API Boilerplate

A production-grade, cloud-ready .NET 9 API boilerplate with clean architecture, built for deployment on Coolify/Hetzner or any container platform.

> **📖 Design Decisions:** This boilerplate makes specific architectural choices (Minimal APIs, custom auth, clean architecture). See [DESIGN_DECISIONS.md](DESIGN_DECISIONS.md) for rationale and alternatives (Controllers, ASP.NET Core Identity, etc.).

## Features

### Architecture & Patterns
- **Clean Architecture** - Domain, Application, Infrastructure, and Web layers
- **CQRS** - Command Query Responsibility Segregation with MediatR
- **Result Pattern** - Type-safe error handling without exceptions
- **Repository Pattern** - Abstracted data access with interfaces
- **Dependency Injection** - Throughout the application

### Authentication & Authorization
- **JWT Authentication** - Access tokens with configurable expiry
- **Refresh Tokens** - Server-side stored refresh tokens
- **Role-Based Authorization** - User, Admin, SuperAdmin roles
- **Policy-Based Authorization** - Custom policies (AdminOnly, CanManageUsers)
- **Secure Password Hashing** - ASP.NET Core Identity PasswordHasher

### API Features
- **Minimal APIs** - Feature-based endpoint organization
- **Swagger/OpenAPI** - Interactive API documentation with JWT support
- **FluentValidation** - Request validation with automatic pipeline behavior
- **Health Checks** - `/health` (liveness) and `/health/ready` (readiness with DB check)
- **CORS** - Configurable cross-origin resource sharing
- **ProblemDetails** - RFC 7807 compliant error responses

### Data & Persistence
- **PostgreSQL** - Production-ready database
- **Entity Framework Core 9** - Modern ORM with migrations
- **Separate Entity Configurations** - Clean, maintainable entity mappings
- **Automatic Timestamps** - CreatedAt, UpdatedAt tracking

### Logging & Observability
- **Serilog** - Structured logging
- **JSON Formatting** - CompactJsonFormatter for production
- **Request Logging** - Automatic HTTP request/response logging
- **Correlation IDs** - Traced through all logs and errors

### Cloud & Container Ready
- **Docker** - Multi-stage Dockerfile with non-root user
- **Docker Compose** - Full stack with PostgreSQL
- **Configurable PORT** - Respects `PORT` environment variable (Coolify compatible)
- **Health Checks** - Built-in Docker healthcheck
- **Environment-based Config** - appsettings.json hierarchy + environment variables

### Testing
- **xUnit** - Modern testing framework
- **FluentAssertions** - Readable test assertions
- **NSubstitute** - Mocking framework
- **Integration Tests** - WebApplicationFactory with Testcontainers
- **Unit Tests** - Command/Query handler tests with examples

### DevOps & CI/CD
- **GitHub Actions** - Automated CI/CD pipeline
- **Automated Testing** - Unit and integration tests in CI
- **Container Registry** - Builds and pushes Docker images
- **Makefile** - Common development tasks
- **PowerShell Scripts** - Cross-platform build, test, and migration scripts

## Project Structure

```
.
├── src/
│   ├── Domain/                      # Enterprise business rules
│   │   ├── Common/                  # Base entities and interfaces
│   │   ├── Entities/                # Domain entities (User, Project, RefreshToken)
│   │   └── Enums/                   # Domain enumerations
│   ├── Application/                 # Application business rules
│   │   ├── Common/                  # Shared application logic
│   │   │   ├── Behaviors/           # MediatR pipeline behaviors
│   │   │   ├── Interfaces/          # Application interfaces
│   │   │   └── Models/              # Result pattern, DTOs
│   │   └── Features/                # Feature-based organization
│   │       ├── Auth/                # Authentication commands/queries
│   │       └── Projects/            # Projects CRUD operations
│   ├── Infrastructure/              # External concerns
│   │   ├── Data/                    # EF Core DbContext and configurations
│   │   └── Services/                # Service implementations
│   └── Web/                         # API entry point
│       ├── Features/                # API endpoints by feature
│       ├── Middleware/              # Custom middleware
│       └── Services/                # Web-specific services
├── tests/
│   ├── Application.UnitTests/       # Unit tests
│   └── Web.IntegrationTests/        # Integration tests
├── Dockerfile                       # Multi-stage production Dockerfile
├── docker-compose.yml               # Local development stack
├── Makefile                         # Development commands
└── scripts/                         # Helper scripts
```

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- [Docker](https://www.docker.com/get-started) and Docker Compose
- [PostgreSQL](https://www.postgresql.org/) (for local development without Docker)
- [Entity Framework Core CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) (optional, for migrations)

## Getting Started

### 1. Clone the Repository

```bash
git clone <your-repo-url>
cd dotnet-boilerplate
```

### 2. Run with Docker Compose (Recommended)

```bash
# Start the entire stack (API + PostgreSQL)
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop services
docker-compose down
```

The API will be available at `http://localhost:8080`
Swagger UI: `http://localhost:8080/swagger`

### 3. Run Locally with .NET CLI

**Step 1: Start PostgreSQL**

```bash
# Using Docker
docker run -d \
  --name postgres-dev \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=dotnetboilerplate \
  -p 5432:5432 \
  postgres:16-alpine

# OR use your local PostgreSQL installation
```

**Step 2: Update Connection String**

Edit `src/Web/appsettings.Development.json` or set environment variable:

```bash
export ConnectionStrings__Default="Host=localhost;Port=5432;Database=dotnetboilerplate;Username=postgres;Password=postgres"
```

**Step 3: Run Migrations**

```bash
# Using Makefile
make migrate

# OR using dotnet CLI
dotnet ef database update --project src/Infrastructure/Infrastructure.csproj --startup-project src/Web/Web.csproj

# OR using PowerShell script
.\scripts\migrate.ps1 -Action update
```

**Step 4: Run the Application**

```bash
# Using Makefile
make run

# OR using dotnet CLI
dotnet run --project src/Web/Web.csproj

# OR with hot reload
make watch
```

The API will be available at `http://localhost:5000`
Swagger UI: `http://localhost:5000/swagger`

## Development Commands

### Using Makefile

```bash
make help              # Show all available commands
make restore           # Restore NuGet packages
make build             # Build the solution
make test              # Run all tests
make test-unit         # Run unit tests only
make test-integration  # Run integration tests only
make run               # Run the application
make watch             # Run with hot reload
make clean             # Clean build artifacts
make migrate           # Apply database migrations
make migration-add NAME=MigrationName  # Add new migration
make migration-remove  # Remove last migration
make docker-build      # Build Docker image
make docker-up         # Start Docker Compose
make docker-down       # Stop Docker Compose
make format            # Format code
```

### Using PowerShell Scripts

```powershell
# Build
.\scripts\build.ps1

# Test
.\scripts\test.ps1
.\scripts\test.ps1 -UnitOnly
.\scripts\test.ps1 -IntegrationOnly

# Migrations
.\scripts\migrate.ps1 -Action add -Name InitialCreate
.\scripts\migrate.ps1 -Action update
.\scripts\migrate.ps1 -Action remove
.\scripts\migrate.ps1 -Action list
```

## Database Migrations

### Create a New Migration

```bash
# Using Makefile
make migration-add NAME=AddNewFeature

# Using dotnet CLI
dotnet ef migrations add AddNewFeature \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Web/Web.csproj

# Using PowerShell
.\scripts\migrate.ps1 -Action add -Name AddNewFeature
```

### Apply Migrations

```bash
# Using Makefile
make migrate

# Using dotnet CLI
dotnet ef database update \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Web/Web.csproj

# Using PowerShell
.\scripts\migrate.ps1 -Action update
```

### Remove Last Migration

```bash
# Using Makefile
make migration-remove

# Using PowerShell
.\scripts\migrate.ps1 -Action remove
```

## API Endpoints

### Authentication

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/register` | Register new user | No |
| POST | `/api/auth/login` | Login with credentials | No |
| POST | `/api/auth/refresh` | Refresh access token | No |
| GET | `/api/auth/me` | Get current user info | Yes |

### Projects (Example Feature)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/projects` | List user's projects (paginated) | Yes |
| GET | `/api/projects/{id}` | Get project by ID | Yes |
| POST | `/api/projects` | Create new project | Yes |
| PUT | `/api/projects/{id}` | Update project | Yes |
| DELETE | `/api/projects/{id}` | Delete project | Yes |

### Health Checks

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Liveness probe |
| GET | `/health/ready` | Readiness probe (includes DB check) |

### Example Request

```bash
# Register
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123",
    "firstName": "John",
    "lastName": "Doe"
  }'

# Login
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123"
  }'

# Create Project (with JWT token)
curl -X POST http://localhost:8080/api/projects \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -d '{
    "name": "My Project",
    "description": "Project description",
    "startDate": "2024-01-01T00:00:00Z"
  }'
```

## Deployment

### Deploying to Coolify on Hetzner

1. **Push your code to Git** (GitHub, GitLab, etc.)

2. **Create a new service in Coolify**:
   - Select "Docker Compose" or "Dockerfile"
   - Point to your repository
   - Set the Dockerfile path to `./Dockerfile`

3. **Configure Environment Variables** in Coolify:

   ```env
   ASPNETCORE_ENVIRONMENT=Production
   PORT=8080
   ConnectionStrings__Default=Host=your-db-host;Port=5432;Database=yourdb;Username=user;Password=pass
   Jwt__Secret=YourSuperSecretKeyThatIsAtLeast256BitsLong!
   Jwt__Issuer=DotNetBoilerplate
   Jwt__Audience=DotNetBoilerplateUsers
   Jwt__ExpiryMinutes=15
   ```

4. **Set up PostgreSQL** (if not using external DB):
   - Add a PostgreSQL service in Coolify
   - Note the connection details
   - Update `ConnectionStrings__Default`

5. **Configure Health Checks**:
   - Liveness: `/health`
   - Readiness: `/health/ready`
   - Port: `8080` (or your configured PORT)

6. **Deploy**:
   - Coolify will build and deploy automatically
   - Monitor logs for any issues

### Environment Variables Reference

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `ASPNETCORE_ENVIRONMENT` | Environment (Development/Production) | Production | No |
| `PORT` | HTTP listening port | 8080 | No |
| `ASPNETCORE_URLS` | URL bindings | http://+:8080 | No |
| `ConnectionStrings__Default` | PostgreSQL connection string | - | Yes |
| `Jwt__Secret` | JWT signing key (min 256 bits) | - | Yes |
| `Jwt__Issuer` | JWT issuer | DotNetBoilerplate | No |
| `Jwt__Audience` | JWT audience | DotNetBoilerplateUsers | No |
| `Jwt__ExpiryMinutes` | Access token expiry | 15 | No |
| `Cors__AllowedOrigins__0` | CORS allowed origin | - | No |

### Docker Build & Run

```bash
# Build image
docker build -t dotnet-boilerplate:latest .

# Run container
docker run -d \
  -p 8080:8080 \
  -e ConnectionStrings__Default="Host=db;Port=5432;Database=mydb;Username=user;Password=pass" \
  -e Jwt__Secret="YourSecretKey" \
  --name dotnet-api \
  dotnet-boilerplate:latest

# View logs
docker logs -f dotnet-api
```

## Testing

### Run All Tests

```bash
make test
# OR
dotnet test
```

### Run Unit Tests Only

```bash
make test-unit
# OR
dotnet test tests/Application.UnitTests/Application.UnitTests.csproj
```

### Run Integration Tests Only

```bash
make test-integration
# OR
dotnet test tests/Web.IntegrationTests/Web.IntegrationTests.csproj
```

Integration tests use Testcontainers to spin up a real PostgreSQL instance automatically.

## Configuration

### appsettings.json Hierarchy

1. `appsettings.json` - Base configuration
2. `appsettings.{Environment}.json` - Environment-specific overrides
3. Environment variables - Highest precedence

### Environment Variable Format

For nested JSON configs, use double underscores (`__`):

```bash
# JSON: { "ConnectionStrings": { "Default": "..." } }
ConnectionStrings__Default="Host=localhost;..."

# JSON: { "Jwt": { "Secret": "..." } }
Jwt__Secret="YourSecret"

# JSON: { "Cors": { "AllowedOrigins": ["url1", "url2"] } }
Cors__AllowedOrigins__0="http://localhost:3000"
Cors__AllowedOrigins__1="http://localhost:5173"
```

## Security Best Practices

✅ **Implemented:**
- Non-root Docker user
- Secure password hashing (ASP.NET Core Identity PasswordHasher)
- JWT with short-lived access tokens
- Refresh tokens stored server-side
- CORS configuration
- Request validation with FluentValidation
- Correlation IDs for request tracing
- ProblemDetails error responses (no stack traces in production)

⚠️ **Before Production:**
1. Change `Jwt__Secret` to a strong random value (min 256 bits)
2. Update `POSTGRES_PASSWORD` to a strong password
3. Configure `Cors__AllowedOrigins` with your actual frontend domains
4. Review and restrict CORS policy
5. Enable HTTPS (handled by reverse proxy in Coolify)
6. Set `ASPNETCORE_ENVIRONMENT=Production`
7. Review logging configuration (no sensitive data in logs)

## Adding New Features

### 1. Create Domain Entity

```csharp
// src/Domain/Entities/Task.cs
public class Task : BaseAuditableEntity
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
}
```

### 2. Add Entity Configuration

```csharp
// src/Infrastructure/Data/Configurations/TaskConfiguration.cs
public class TaskConfiguration : IEntityTypeConfiguration<Task>
{
    public void Configure(EntityTypeBuilder<Task> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
        // ... more configuration
    }
}
```

### 3. Update DbContext

```csharp
// src/Infrastructure/Data/ApplicationDbContext.cs
public DbSet<Task> Tasks => Set<Task>();
```

### 4. Create Command/Query

```csharp
// src/Application/Features/Tasks/Commands/CreateTask/CreateTaskCommand.cs
public record CreateTaskCommand(string Title, string? Description, Guid ProjectId)
    : IRequest<Result<TaskDto>>;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Result<TaskDto>>
{
    // Implementation
}
```

### 5. Add Validator

```csharp
// src/Application/Features/Tasks/Commands/CreateTask/CreateTaskCommandValidator.cs
public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}
```

### 6. Create Endpoints

```csharp
// src/Web/Features/Tasks/TasksEndpoints.cs
public static class TasksEndpoints
{
    public static IEndpointRouteBuilder MapTasksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks").RequireAuthorization();
        group.MapPost("/", CreateTask);
        return app;
    }
}
```

### 7. Register Endpoints

```csharp
// src/Web/Program.cs
app.MapTasksEndpoints();
```

### 8. Create Migration

```bash
make migration-add NAME=AddTasksTable
make migrate
```

## Troubleshooting

### Database Connection Issues

```bash
# Check if PostgreSQL is running
docker ps | grep postgres

# Test connection
psql -h localhost -U postgres -d dotnetboilerplate

# View connection string
echo $ConnectionStrings__Default
```

### Migration Issues

```bash
# List all migrations
dotnet ef migrations list --project src/Infrastructure --startup-project src/Web

# Remove all migrations and start fresh
rm -rf src/Infrastructure/Migrations/
make migration-add NAME=InitialCreate
make migrate
```

### Docker Issues

```bash
# View API logs
docker-compose logs -f api

# Restart services
docker-compose restart

# Clean rebuild
docker-compose down -v
docker-compose build --no-cache
docker-compose up -d
```

## License

This project is licensed under the MIT License.

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Support

For issues, questions, or contributions, please open an issue on GitHub.
