# POS Backend API Development Guidelines

Auto-generated from all feature plans. Last updated: 2025-10-05

## Active Technologies

**Language/Framework**: C# 13 / .NET 9, ASP.NET Core 9 Web API
**Architecture**: Clean Architecture (Domain, Application, Infrastructure, WebApi layers)
**Patterns**: CQRS (MediatR), Repository, Domain Events
**Database**: PostgreSQL 16+ (primary), Redis 7+ (caching, sessions, jobs)
**Key Libraries**:
- MediatR 12.x - CQRS pattern
- Entity Framework Core 9 - ORM
- SignalR - Real-time notifications
- FluentValidation - Input validation
- AutoMapper - Object mapping
- Serilog - Structured logging
- Hangfire - Background jobs

**Testing**: xUnit, FluentAssertions, Moq, Testcontainers, Bogus
**Deployment**: Docker containers, Kubernetes

## Project Structure

```
src/
├── Domain/                     # Pure business logic, entities, domain events
│   ├── Entities/
│   │   ├── Sales/             # Order, OrderItem, Payment, Shift
│   │   ├── Inventory/         # Product, ProductVariant, InventoryLevel
│   │   ├── Customers/         # Customer, CustomerGroup, LoyaltyTransaction
│   │   ├── Employees/         # User, Role, Permission, Commission
│   │   ├── Stores/            # Store, StoreSettings
│   │   └── Common/            # BaseEntity, IAuditable
│   ├── Events/                # Domain events
│   ├── Exceptions/            # Domain-specific exceptions
│   ├── Interfaces/            # Repository interfaces
│   └── ValueObjects/          # Address, Money, PhoneNumber
│
├── Application/               # Use cases, CQRS handlers, DTOs
│   ├── Common/
│   │   ├── Behaviours/       # MediatR pipeline behaviors
│   │   ├── Interfaces/       # IApplicationDbContext, ICurrentUserService
│   │   ├── Mappings/         # AutoMapper profiles
│   │   └── Models/           # Result<T>, PaginatedList<T>
│   ├── Features/
│   │   ├── Auth/             # Login, JWT, permissions
│   │   ├── POS/              # Sales, payments, receipts
│   │   ├── Inventory/        # Stock management
│   │   ├── Customers/        # CRM, loyalty
│   │   ├── Employees/        # User management
│   │   ├── Reports/          # Analytics
│   │   └── Integrations/     # VNPAY, GHN, GHTK
│   └── DependencyInjection.cs
│
├── Infrastructure/            # Data access, external services
│   ├── Persistence/
│   │   ├── Configurations/   # EF Core entity configs
│   │   ├── Migrations/       # Database migrations
│   │   ├── Repositories/     # Repository implementations
│   │   └── ApplicationDbContext.cs
│   ├── Identity/             # JWT, user management
│   ├── Services/
│   │   ├── Caching/          # Redis service
│   │   ├── Notifications/    # SignalR service
│   │   ├── Jobs/             # Hangfire background jobs
│   │   └── External/         # Third-party integrations
│   ├── Localization/         # Vietnamese resources
│   └── DependencyInjection.cs
│
└── WebApi/                    # API endpoints, middleware
    ├── Controllers/           # REST endpoints
    ├── Hubs/                 # SignalR hubs
    ├── Middleware/           # Exception, rate limiting, audit
    ├── Filters/              # Authorization policies
    └── Program.cs

tests/
├── Domain.UnitTests/         # Entity unit tests
├── Application.UnitTests/    # Handler unit tests
├── Application.IntegrationTests/  # Flow tests with Testcontainers
└── WebApi.ContractTests/     # API contract tests
```

## Commands

### Development
```bash
# Build solution
dotnet build

# Run WebApi project
dotnet run --project src/WebApi

# Run tests
dotnet test

# Create migration
dotnet ef migrations add <MigrationName> --project src/Infrastructure --startup-project src/WebApi

# Update database
dotnet ef database update --project src/Infrastructure --startup-project src/WebApi

# Generate OpenAPI spec
dotnet swagger tofile --output swagger.json src/WebApi/bin/Debug/net9.0/WebApi.dll v1
```

### Docker
```bash
# Build image
docker build -t pos-api .

# Run with compose
docker-compose up -d

# View logs
docker-compose logs -f api
```

## Code Style

### C# Conventions
- **Nullable Reference Types**: Enabled across all projects
- **File-scoped namespaces**: Use `namespace MyNamespace;` (C# 10+)
- **Primary constructors**: Prefer for simple classes (C# 12+)
- **Target-typed new**: Use `new()` when type is obvious
- **Pattern matching**: Utilize switch expressions and property patterns
- **Async/await**: All I/O operations must be async
- **Records**: Use for DTOs and value objects

### Clean Architecture Rules
- **Domain**: No dependencies on other layers or external packages
- **Application**: Depends only on Domain, defines interfaces for infrastructure
- **Infrastructure**: Implements Application interfaces, depends on Application
- **WebApi**: Depends only on Infrastructure (transitive to all layers)

### CQRS Pattern
```csharp
// Command (state-changing)
public record CreateOrderCommand(Guid StoreId, List<OrderItemDto> Items) : IRequest<Result<Guid>>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    // Implementation
}

// Query (read-only)
public record GetProductQuery(Guid Id) : IRequest<Result<ProductDto>>;

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, Result<ProductDto>>
{
    // Implementation with AsNoTracking()
}
```

### Entity Configuration
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}

// EF Core Configuration
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasQueryFilter(p => !p.IsDeleted); // Soft delete
        builder.HasIndex(p => p.StoreId); // Multi-tenancy
    }
}
```

### Validation
```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SKU).NotEmpty().Matches("^[A-Z0-9-]+$");
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
```

## Performance Guidelines
- **Payment transactions**: < 3 seconds
- **Complex reports**: < 10 seconds
- **CRUD operations**: < 200ms (p95)
- **API capacity**: 1000 requests/minute
- **Concurrent transactions**: 50+/minute

**Optimization Techniques**:
- Use `AsNoTracking()` for read-only queries
- Implement Redis caching for frequently accessed data
- Use compiled queries for hot paths
- Leverage PostgreSQL indexes and partitioning
- Apply pagination for large result sets

## Security Practices
- **JWT Authentication**: 15-minute access tokens, 7-day refresh tokens
- **Password Hashing**: BCrypt with work factor 12
- **Authorization**: Role-based (RBAC) with claims
- **Input Validation**: FluentValidation on all commands
- **Rate Limiting**: 1000 req/min general, 10 req/min login
- **SQL Injection**: Use parameterized queries (EF Core)
- **Audit Logging**: Track all critical operations

## Recent Changes
- **001-create-project-backend**: Initial Clean Architecture setup with .NET 9, CQRS pattern, PostgreSQL, Redis, and SignalR

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->