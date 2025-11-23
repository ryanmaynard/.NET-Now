# Design Decisions

This document explains key architectural and implementation decisions in this boilerplate.

## Table of Contents
- [Authentication Approach](#authentication-approach)
- [Minimal APIs vs Controllers](#minimal-apis-vs-controllers)
- [Custom User Management vs ASP.NET Core Identity](#custom-user-management-vs-aspnet-core-identity)
- [Reflection in Service Registration](#reflection-in-service-registration)
- [Clean Architecture Layers](#clean-architecture-layers)

---

## Authentication Approach

### Decision: Custom JWT Authentication with Refresh Tokens

**Why not ASP.NET Core Identity?**

ASP.NET Core Identity is a comprehensive solution, but this boilerplate uses a lightweight custom approach for these reasons:

**Pros of Custom Approach:**
- **Simplicity** - Less complexity, easier to understand the entire auth flow
- **Lightweight** - No hidden magic, no unnecessary tables/complexity
- **API-First** - Designed for stateless JWT authentication, not cookie-based auth
- **Portability** - Easy to understand and modify for specific needs
- **Modern** - Follows modern JWT + refresh token patterns used in many APIs

**When to Use ASP.NET Core Identity Instead:**
- You need built-in support for external auth providers (Google, Facebook, etc.)
- You need Two-Factor Authentication (2FA) out of the box
- You need email confirmation workflows
- You need password reset flows with tokens
- You want lockout policies and other security features pre-built
- You're building a traditional MVC app with cookies

**Migration Path:**
If you need ASP.NET Core Identity later, you can:
1. Install `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
2. Make `User` extend `IdentityUser`
3. Make `ApplicationDbContext` extend `IdentityDbContext<User>`
4. Use `UserManager<User>` and `SignInManager<User>`
5. Remove custom authentication handlers

**References:**
- [ASP.NET Core Identity Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [JWT Bearer Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)

---

## Minimal APIs vs Controllers

### Decision: Minimal APIs with Feature-Based Organization

This boilerplate uses **Minimal APIs** (introduced in .NET 6) organized by feature rather than traditional Controllers.

**Why Minimal APIs?**

**Pros:**
- **Modern .NET Approach** - Minimal APIs are the recommended approach for new .NET APIs (6+)
- **Less Boilerplate** - No controller classes, attributes everywhere, or base class inheritance
- **Performance** - Slightly better performance (no controller activation overhead)
- **Flexibility** - Easier to organize by feature rather than by controller
- **Clear Dependencies** - Explicit dependencies in endpoint methods

**Cons:**
- **Less Familiar** - Traditional Controllers are more widely known
- **Fewer Built-in Features** - Some attribute-based features require manual implementation
- **Testing** - Slightly different testing approach (though still straightforward)

**When to Use Controllers Instead:**
- Your team is more familiar with MVC/Controllers pattern
- You have complex endpoint groups that benefit from controller-level attributes
- You're migrating from an existing ASP.NET Framework project
- You need certain built-in features like model binding attributes

**Controller Alternative Example:**

If you prefer Controllers, here's how to convert:

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        var result = await _sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { errors = result.Errors });
    }
}
```

**References:**
- [Minimal APIs Overview](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [Controllers vs Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/overview)

---

## Custom User Management vs ASP.NET Core Identity

### Decision: Custom User Entity and Management

**Current Approach:**
- Custom `User` entity in Domain layer
- Custom `IPasswordHasher` using ASP.NET Core's `PasswordHasher<T>` under the hood
- Custom user repository pattern
- Manual role management with `UserRole` enum

**Why Not Full Identity?**
1. **Simplicity** - Easier to understand the entire auth system
2. **Domain-Driven** - User is a domain entity, not an infrastructure concern
3. **Clean Architecture** - Keeps Identity framework out of the domain
4. **Flexibility** - Easy to modify for specific requirements

**What We Still Use from ASP.NET Core:**
- `PasswordHasher<T>` for secure password hashing (via `IPasswordHasher`)
- `[Authorize]` attributes and policy-based authorization
- JWT Bearer authentication middleware

**Trade-offs:**
- ✅ Simpler, more transparent
- ✅ Better for learning
- ✅ Easier to customize
- ❌ Need to implement password reset yourself
- ❌ Need to implement email confirmation yourself
- ❌ Need to implement 2FA yourself
- ❌ No built-in external auth providers

---

## Reflection in Service Registration

### Decision: Use Reflection for MediatR/FluentValidation, Explicit for Infrastructure

**MediatR and FluentValidation Registration:**
```csharp
services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
```

**Why Use Reflection Here?**
1. **Convention Over Configuration** - Standard pattern for these libraries
2. **Maintainability** - Auto-discovers new handlers/validators without manual registration
3. **Type Safety** - Still type-safe at compile time
4. **Performance** - Reflection only happens once at startup, not per-request
5. **Recommended Practice** - This is how MediatR and FluentValidation are designed to be used

**Infrastructure Services Registration:**
```csharp
services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
services.AddScoped<IPasswordHasher, PasswordHasherService>();
services.AddScoped<ITokenService, TokenService>();
```

**Why Explicit Here?**
1. **Clarity** - Clear what services are registered
2. **Performance** - Slightly faster than reflection
3. **Control** - Explicit lifetime management (Singleton/Scoped/Transient)
4. **Debuggability** - Easier to trace service registration

**Best Practice:**
- Use reflection for **convention-based** registrations (handlers, validators)
- Use explicit registration for **infrastructure services** and **important dependencies**

---

## Clean Architecture Layers

### Decision: Domain → Application → Infrastructure → Web

**Layer Responsibilities:**

**Domain Layer (No Dependencies):**
- Entities, Value Objects, Enums
- Domain logic and business rules
- No framework dependencies
- No external concerns

**Application Layer (Depends on: Domain):**
- Use cases (Commands/Queries)
- Interfaces for infrastructure
- DTOs and mapping
- Validation rules
- Business orchestration

**Infrastructure Layer (Depends on: Application, Domain):**
- Database (EF Core)
- External services
- File system
- Email, SMS, etc.
- Interface implementations

**Web Layer (Depends on: Application, Infrastructure):**
- API endpoints
- Middleware
- Dependency injection setup
- Configuration
- Entry point (Program.cs)

**Benefits:**
- **Testability** - Easy to test domain and application logic
- **Maintainability** - Clear separation of concerns
- **Flexibility** - Can swap infrastructure without touching domain
- **Independence** - Domain doesn't depend on frameworks

**Trade-offs:**
- ✅ Better for complex domains
- ✅ Better long-term maintainability
- ✅ Forces good design
- ❌ More initial setup
- ❌ Can feel like over-engineering for simple CRUD
- ❌ More files and folders

**When This Makes Sense:**
- Medium to large applications
- Complex business logic
- Long-term maintenance expected
- Team with multiple developers
- Learning/reference project

**When to Simplify:**
- Very small APIs (< 10 endpoints)
- Simple CRUD operations
- Prototype or proof-of-concept
- Single developer, short-term project

---

## Additional Notes

### Feature-Based Organization

Instead of organizing by technical concern (Controllers, Services, Models), this boilerplate organizes by **feature**:

```
Features/
  Auth/
    Commands/
      Register/
        RegisterCommand.cs
        RegisterCommandHandler.cs
        RegisterCommandValidator.cs
    Queries/
      GetCurrentUser/
        GetCurrentUserQuery.cs
        GetCurrentUserQueryHandler.cs
```

**Benefits:**
- Related code is grouped together
- Easy to find all code related to a feature
- Can delete entire feature folders cleanly
- Better for microservices extraction

### Result Pattern

Uses a `Result<T>` pattern instead of throwing exceptions for business logic failures:

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string[] Errors { get; }
}
```

**Benefits:**
- Type-safe error handling
- No exception overhead for business failures
- Explicit success/failure handling
- Better API responses

**When to Throw Exceptions:**
- Truly exceptional circumstances
- Infrastructure failures
- Invalid state that should never occur

---

## Feedback Welcome

These decisions were made for this specific boilerplate. Your project might have different needs. Consider:
- Your team's expertise
- Project complexity and size
- Long-term maintenance plans
- Performance requirements
- Integration with existing systems

Feel free to adapt this boilerplate to your needs!
