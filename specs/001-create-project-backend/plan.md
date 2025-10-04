# Implementation Plan: Backend API for POS Application

**Branch**: `001-create-project-backend` | **Date**: 2025-10-04 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-create-project-backend/spec.md`

## Execution Flow (/plan command scope)
```
1. ✅ Load feature spec from Input path
2. ✅ Fill Technical Context (user provided: Clean Architecture, ASP.NET Core 9, MediatR, SignalR, PostgreSQL, Redis)
3. ✅ Fill Constitution Check section
4. ✅ Evaluate Constitution Check section
5. ⏳ Execute Phase 0 → research.md
6. ⏳ Execute Phase 1 → contracts, data-model.md, quickstart.md, CLAUDE.md
7. ⏳ Re-evaluate Constitution Check
8. ⏳ Plan Phase 2 → Describe task generation approach
9. STOP - Ready for /tasks command
```

## Summary

Develop a comprehensive RESTful backend API for a Point of Sale (POS) system targeting medium-chain retail stores (6-20 stores, 50-100 employees). The system provides modules for sales transactions (POS), inventory management, customer relationship management (CRM), employee management, and reporting/analytics. Technical approach follows Clean Architecture with ASP.NET Core 9, implementing CQRS pattern via MediatR, real-time capabilities with SignalR, PostgreSQL for persistent storage, and Redis for caching and session management.

## Technical Context

**Language/Version**: C# 13 / .NET 9
**Framework**: ASP.NET Core 9 Web API
**Architecture**: Clean Architecture (Domain, Application, Infrastructure, Presentation layers)
**Primary Dependencies**:
- MediatR 12.x (CQRS pattern implementation)
- SignalR (real-time notifications)
- Entity Framework Core 9 (ORM)
- FluentValidation (input validation)
- AutoMapper (object mapping)
- Serilog (structured logging)
**Storage**: PostgreSQL 16+ (primary), Redis 7+ (caching, session, job queue)
**Testing**: xUnit, FluentAssertions, Moq, Testcontainers, Bogus
**Target Platform**: Linux containers (Docker), Kubernetes deployment
**Project Type**: Web API (backend-only)
**Performance Goals**:
- < 3s payment transaction processing
- < 10s complex report generation
- 50+ concurrent transactions/minute
- 1000 requests/minute general API capacity
**Constraints**:
- 99.5% uptime during business hours
- 7+ year data retention compliance
- Vietnamese language support (localization)
- Multi-store isolation & centralized reporting
- < 200ms p95 for CRUD operations
**Scale/Scope**:
- 6-20 stores initial deployment
- 50-100 employees
- ~10k products with variants
- ~500-1000 daily transactions per store
- 62 functional requirements across 8 modules
- 22 core domain entities

## Constitution Check

*Using sensible defaults in absence of explicit constitution*

### Core Principles Applied

**✅ Separation of Concerns (Clean Architecture)**
- Domain layer: Pure business logic, entity models, domain events
- Application layer: Use cases (Commands/Queries), interfaces, DTOs
- Infrastructure layer: Data access, external integrations, caching
- Presentation layer: API controllers, SignalR hubs, middleware

**✅ CQRS Pattern (MediatR)**
- Commands: State-changing operations (CreateOrder, UpdateInventory)
- Queries: Read-only operations (GetProducts, GenerateReport)
- Clear separation improves scalability and testability

**✅ Test-Driven Development**
- Contract tests for all API endpoints
- Integration tests for critical flows (sales, inventory)
- Unit tests for business logic
- Performance tests for SLA compliance

**✅ Security First**
- JWT authentication for all endpoints
- Role-based authorization (RBAC)
- Input validation via FluentValidation
- SQL injection prevention (parameterized queries)
- Rate limiting middleware

**✅ Observability**
- Structured logging (Serilog) to ELK stack
- Health check endpoints for monitoring
- Audit trail for critical operations
- Performance metrics collection

**Gate Status**: ✅ PASS (no constitutional violations)

## Project Structure

### Documentation (this feature)
```
specs/001-create-project-backend/
├── plan.md              # This file (/plan command output)
├── research.md          # Phase 0 output (/plan command)
├── data-model.md        # Phase 1 output (/plan command)
├── quickstart.md        # Phase 1 output (/plan command)
├── contracts/           # Phase 1 output (/plan command)
│   ├── openapi.yaml     # Full OpenAPI 3.1 specification
│   ├── auth.yaml        # Authentication endpoints
│   ├── pos.yaml         # POS module endpoints
│   ├── inventory.yaml   # Inventory management endpoints
│   ├── crm.yaml         # Customer management endpoints
│   ├── employees.yaml   # Employee management endpoints
│   ├── reports.yaml     # Reporting endpoints
│   └── integrations.yaml # External integration endpoints
└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)
```

### Source Code (repository root)

```
src/
├── Domain/
│   ├── Entities/
│   │   ├── Sales/          # Order, OrderItem, Payment, Shift
│   │   ├── Inventory/      # Product, ProductVariant, InventoryLevel, etc.
│   │   ├── Customers/      # Customer, CustomerGroup, LoyaltyTransaction
│   │   ├── Employees/      # User, Role, Permission, Commission
│   │   ├── Stores/         # Store, StoreSettings
│   │   └── Common/         # BaseEntity, IAuditable
│   ├── Events/             # Domain events for event sourcing
│   ├── Exceptions/         # Domain-specific exceptions
│   ├── Interfaces/         # Repository interfaces
│   └── ValueObjects/       # Address, Money, PhoneNumber
│
├── Application/
│   ├── Common/
│   │   ├── Behaviours/     # Pipeline behaviors (logging, validation, transactions)
│   │   ├── Interfaces/     # IApplicationDbContext, ICurrentUserService
│   │   ├── Mappings/       # AutoMapper profiles
│   │   └── Models/         # Result<T>, PaginatedList<T>
│   ├── Features/
│   │   ├── Auth/           # Commands/Queries for authentication
│   │   ├── POS/            # Commands/Queries for POS operations
│   │   ├── Inventory/      # Commands/Queries for inventory
│   │   ├── Customers/      # Commands/Queries for CRM
│   │   ├── Employees/      # Commands/Queries for employee mgmt
│   │   ├── Reports/        # Queries for reports & analytics
│   │   └── Integrations/   # Commands for external integrations
│   └── DependencyInjection.cs
│
├── Infrastructure/
│   ├── Persistence/
│   │   ├── Configurations/ # EF Core entity configurations
│   │   ├── Migrations/     # Database migrations
│   │   ├── Repositories/   # Repository implementations
│   │   └── ApplicationDbContext.cs
│   ├── Identity/           # JWT token generation, user management
│   ├── Services/
│   │   ├── Caching/        # Redis cache service
│   │   ├── Notifications/  # SignalR notification service
│   │   ├── Jobs/           # Background job processing (Hangfire)
│   │   └── External/       # VNPAY, GHN, GHTK integrations
│   ├── Localization/       # Vietnamese resource files
│   └── DependencyInjection.cs
│
└── WebApi/
    ├── Controllers/
    │   ├── AuthController.cs
    │   ├── POSController.cs
    │   ├── InventoryController.cs
    │   ├── CustomersController.cs
    │   ├── EmployeesController.cs
    │   ├── ReportsController.cs
    │   └── IntegrationsController.cs
    ├── Hubs/               # SignalR hubs
    │   ├── NotificationHub.cs
    │   └── DashboardHub.cs
    ├── Middleware/
    │   ├── ExceptionHandlingMiddleware.cs
    │   ├── RateLimitingMiddleware.cs
    │   └── AuditLoggingMiddleware.cs
    ├── Filters/            # Action filters, authorization policies
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── Program.cs
    └── DependencyInjection.cs

tests/
├── Domain.UnitTests/      # Domain entity unit tests
├── Application.UnitTests/  # Command/Query handler unit tests
├── Application.IntegrationTests/
│   ├── POSTests/          # POS flow integration tests
│   ├── InventoryTests/    # Inventory operation tests
│   └── Infrastructure/    # Testcontainers setup
└── WebApi.ContractTests/  # API contract tests (endpoint schemas)
```

**Structure Decision**: Web API project following Clean Architecture with clear layer separation. Domain layer contains business entities and rules. Application layer implements CQRS with MediatR handlers. Infrastructure layer handles data persistence (PostgreSQL via EF Core), caching (Redis), external integrations, and background jobs. WebApi layer exposes RESTful endpoints and SignalR hubs.

## Phase 0: Outline & Research

**Objective**: Research and document technical decisions for all aspects of the POS backend system.

### Research Areas

1. **Clean Architecture Implementation in .NET 9**
   - Layer dependency rules and enforcement
   - Cross-cutting concerns handling
   - Dependency injection setup across layers

2. **CQRS with MediatR**
   - Command/Query handler patterns
   - Pipeline behaviors (validation, logging, transaction)
   - Notification handlers for domain events

3. **PostgreSQL Schema Design**
   - Multi-tenancy strategy (store isolation)
   - Partitioning strategy for order/transaction tables (7-year retention)
   - Indexing for high-volume queries (reports)
   - Full-text search for product/customer search

4. **Redis Usage Patterns**
   - Session storage for JWT tokens
   - Distributed caching strategy (cache-aside pattern)
   - Job queue for background tasks (retry logic for failed integrations)
   - Pub/sub for SignalR backplane

5. **SignalR Real-time Features**
   - Notification hub for low stock alerts
   - Dashboard hub for real-time metrics
   - Scalability with Redis backplane

6. **External Integrations**
   - VNPAY payment gateway integration patterns
   - GHN/GHTK shipping provider API clients
   - Retry policies for failed operations (Polly)
   - Webhook handling for async notifications

7. **Security & Authentication**
   - JWT token generation & refresh strategy
   - Role-based authorization with claims
   - Password hashing (BCrypt/Argon2)
   - Rate limiting strategies (per-user, per-endpoint)

8. **Performance Optimization**
   - EF Core query optimization (no tracking, compiled queries)
   - Pagination best practices
   - Caching strategies (entity caching, query result caching)
   - Async/await patterns for I/O operations

9. **Localization (Vietnamese)**
   - Resource file structure
   - Request culture middleware
   - Validation message localization

10. **Testing Strategy**
    - Testcontainers for PostgreSQL/Redis in integration tests
    - Contract testing with OpenAPI schema validation
    - Performance testing approach (k6/JMeter)

### Research Tasks
- Research Clean Architecture in .NET 9 and best practices for layer separation
- Research MediatR pipeline behaviors for cross-cutting concerns (validation, logging, transactions)
- Research PostgreSQL multi-tenancy patterns for store isolation
- Research table partitioning strategies for 7-year data retention compliance
- Research Redis caching patterns and invalidation strategies
- Research SignalR scalability with Redis backplane for multi-instance deployment
- Research Polly retry policies for external integration resilience
- Research JWT authentication with refresh token rotation
- Research EF Core performance optimization techniques
- Research Testcontainers setup for integration testing with PostgreSQL

**Output**: `research.md` with consolidated findings

## Phase 1: Design & Contracts

*Prerequisites: research.md complete*

### 1. Data Model (`data-model.md`)

Extract and formalize all 22 entities from specification:

**Sales Module**:
- Order (aggregate root)
- OrderItem
- Payment
- Shift
- PaymentMethod

**Inventory Module**:
- Product (aggregate root)
- ProductVariant
- Category
- Brand
- InventoryLevel
- InventoryReceipt
- InventoryReceiptItem
- InventoryIssue
- InventoryIssueItem
- Stocktake
- StocktakeItem

**Customer Module**:
- Customer (aggregate root)
- CustomerGroup
- LoyaltyTransaction
- Debt

**Employee Module**:
- User/Employee
- Role
- Permission
- Commission

**Store Module**:
- Store
- Supplier

Include for each entity:
- Fields with types and constraints
- Relationships (one-to-many, many-to-many)
- Validation rules from requirements
- State transitions (Order: Draft → Completed → Returned)
- Indexes for performance
- Audit fields (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)

### 2. API Contracts (`/contracts/`)

Generate OpenAPI 3.1 specifications organized by module:

**auth.yaml**: Authentication & Authorization
- POST /api/auth/login
- POST /api/auth/refresh
- POST /api/auth/logout
- POST /api/auth/start-shift
- POST /api/auth/end-shift

**pos.yaml**: Point of Sale Operations
- POST /api/orders
- GET /api/orders/{id}
- POST /api/orders/{id}/payments
- POST /api/orders/{id}/hold
- GET /api/orders/held
- POST /api/orders/{id}/complete
- POST /api/orders/{id}/return
- GET /api/orders/{id}/receipt
- GET /api/products/search

**inventory.yaml**: Inventory Management
- GET /api/products
- GET /api/products/{id}
- POST /api/products
- PUT /api/products/{id}
- POST /api/inventory/receipts
- GET /api/inventory/receipts
- POST /api/inventory/issues
- GET /api/inventory/stock-levels
- POST /api/inventory/stocktakes
- POST /api/inventory/stocktakes/{id}/finalize
- POST /api/barcodes/generate

**crm.yaml**: Customer Relationship Management
- GET /api/customers
- POST /api/customers
- GET /api/customers/{id}
- GET /api/customers/search
- GET /api/customers/{id}/orders
- POST /api/customers/{id}/points
- GET /api/customers/{id}/points/history
- POST /api/customers/{id}/debts
- GET /api/customer-groups

**employees.yaml**: Employee Management
- GET /api/employees
- POST /api/employees
- GET /api/employees/{id}
- GET /api/roles
- POST /api/roles
- GET /api/permissions
- GET /api/employees/{id}/commissions

**reports.yaml**: Reporting & Analytics
- GET /api/reports/sales
- GET /api/reports/top-products
- GET /api/reports/inventory-movement
- GET /api/reports/financial
- GET /api/dashboard

**integrations.yaml**: External Integrations
- POST /api/integrations/vnpay/create-qr
- POST /api/integrations/ghn/calculate-fee
- POST /api/integrations/ghn/create-order
- GET /api/integrations/shipping/track/{trackingNumber}
- POST /api/webhooks/order-status

Each endpoint specification includes:
- Request/response schemas
- Authentication requirements
- Validation rules
- Error responses (400, 401, 403, 404, 409, 500)
- Rate limit headers

### 3. Contract Tests

Generate failing contract tests:
- One test class per controller
- Assert request/response schema compliance
- Tests fail initially (no implementation)

### 4. Quickstart Guide (`quickstart.md`)

Step-by-step developer onboarding:
1. Prerequisites (Docker, .NET 9 SDK, PostgreSQL client)
2. Clone and setup
3. Database migration
4. Run tests
5. Start application
6. Execute sample API calls (Postman collection)
7. Verify real-time notifications
8. Performance baseline checks

### 5. Agent Context File

Execute: `.specify/scripts/powershell/update-agent-context.ps1 -AgentType claude`
And Excute: `.specify/scripts/powershell/update-agent-context.ps1 -AgentType copilot`

Generate `CLAUDE.md` at repository root with:
- Project architecture overview
- Technology stack summary
- Key design patterns (CQRS, Clean Architecture)
- Common commands
- Testing approach
- Recent changes

**Outputs**:
- `data-model.md`
- `/contracts/*.yaml` (7 files)
- Failing contract test suite
- `quickstart.md`
- `CLAUDE.md`

## Phase 2: Task Planning Approach

*This section describes what the /tasks command will do - DO NOT execute during /plan*

### Task Generation Strategy

Load `.specify/templates/tasks-template.md` and generate ordered tasks:

**Phase A: Foundation** (Infrastructure Setup)
1. Setup solution structure (4 projects: Domain, Application, Infrastructure, WebApi)
2. Configure dependency injection across layers
3. Setup PostgreSQL connection & EF Core DbContext
4. Setup Redis connection & distributed cache
5. Configure Serilog structured logging
6. Setup health check endpoints

**Phase B: Domain Layer** (Entities & Business Logic)
7-28. Create entity models (22 entities) [P - parallel execution possible]
29. Create value objects (Address, Money, PhoneNumber)
30. Create domain exceptions
31. Create domain events

**Phase C: Application Layer - Core Features**
32-35. Auth module: Commands/Queries (Login, Refresh, StartShift, EndShift)
36-45. POS module: Commands/Queries (10 handlers for order operations)
46-55. Inventory module: Commands/Queries (10 handlers)
56-65. CRM module: Commands/Queries (10 handlers)
66-72. Employee module: Commands/Queries (7 handlers)
73-78. Reports module: Queries only (6 report types)

**Phase D: Infrastructure Layer**
79-100. EF Core configurations (22 entity configurations) [P]
101. Create database migration
102. Redis cache service implementation
103. JWT token service implementation
104-106. External integration clients (VNPAY, GHN, GHTK)
107. Background job setup (Hangfire/Quartz)
108. SignalR notification service

**Phase E: Presentation Layer**
109-115. API Controllers (7 controllers) [P]
116-117. SignalR Hubs (2 hubs)
118-120. Middleware (Exception handling, Rate limiting, Audit logging)

**Phase F: Testing**
121-142. Unit tests for command/query handlers (22 test classes) [P]
143-149. Integration tests for critical flows (7 test classes)
150-156. Contract tests for API endpoints (7 test classes) [P]

**Phase G: Integration & Polish**
157. Configure localization (Vietnamese resources)
158. Performance optimization pass
159. Security audit (OWASP checklist)
160. Generate OpenAPI documentation
161. Setup Docker Compose for local development
162. Run end-to-end test scenarios

### Ordering Strategy

- **TDD order**: Contract tests → Entity tests → Handler tests → Implementation
- **Dependency order**: Domain → Application → Infrastructure → WebApi
- **Parallel execution**: Mark [P] for independent tasks (different files/modules)
- **Critical path**: Auth → POS → Inventory (highest business value first)

### Estimated Output

**~160-170 numbered, ordered tasks** in tasks.md with:
- Clear acceptance criteria
- Dependencies marked
- Parallel execution indicators [P]
- Estimated effort (S/M/L)
- TDD compliance checkpoints

**IMPORTANT**: This phase is executed by the /tasks command, NOT by /plan

## Phase 3+: Future Implementation

*These phases are beyond the scope of the /plan command*

**Phase 3**: Task execution (/tasks command creates tasks.md with ~160-170 tasks)
**Phase 4**: Implementation (execute tasks.md following Clean Architecture & CQRS principles)
**Phase 5**: Validation
- Run all test suites (contract, unit, integration)
- Execute quickstart.md steps
- Performance validation (< 3s payment, < 10s reports, 50 concurrent tx/min)
- Security validation (OWASP checklist)
- Load testing (1000 req/min sustained)

## Complexity Tracking

*No constitutional violations - table not needed*

All design decisions align with Clean Architecture and SOLID principles. CQRS via MediatR adds necessary separation for scalability. External integrations require retry logic as per specification (INT-005). Multi-layered architecture justified by:
- Clear separation of concerns
- Independent testability
- Technology-agnostic domain layer
- Infrastructure flexibility

## Progress Tracking

**Phase Status**:
- [x] Phase 0: Research complete (/plan command) - research.md created
- [x] Phase 1: Design complete (/plan command) - data-model.md created
- [x] Phase 2: Task planning complete (/plan command - approach documented)
- [x] Phase 3: Tasks generated (/tasks command) - tasks.md with 170 tasks created
- [ ] Phase 4: Implementation in progress (execute tasks.md)
- [ ] Phase 5: Validation pending

**Gate Status**:
- [x] Initial Constitution Check: PASS
- [ ] Post-Design Constitution Check: PASS (pending Phase 1 completion)
- [x] All research areas documented (10 areas in research.md)
- [x] Complexity deviations documented (N/A - no deviations)

**Artifacts Created**:
- ✅ plan.md - Implementation plan with technical context and architecture
- ✅ research.md - Comprehensive technical research (10 areas, 25,000+ words)
- ✅ data-model.md - Entity definitions (26 entities with full EF Core specs)
- ✅ tasks.md - Implementation tasks (170 tasks, dependency-ordered, parallel execution optimized)
- ⏳ contracts/ - OpenAPI specifications (defined in plan, to be created during implementation)
- ⏳ quickstart.md - Developer onboarding guide (defined in plan, to be created during implementation)
- ⏳ CLAUDE.md - Agent context file (to be created during implementation)

---
*Implementation plan for Backend API for POS Application | Branch: 001-create-project-backend*
