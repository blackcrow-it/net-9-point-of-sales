# Implementation Tasks: Backend API for POS Application

**Branch**: `001-create-project-backend` | **Date**: 2025-10-05
**Feature**: Backend API for Point of Sale System
**Total Tasks**: 170 | **Estimated Duration**: 8-12 weeks

## Overview

This document provides a complete, dependency-ordered task list for implementing a production-ready POS backend API using Clean Architecture, ASP.NET Core 9, MediatR (CQRS), SignalR, PostgreSQL, and Redis.

**Architecture**: 4-layer Clean Architecture (Domain, Application, Infrastructure, WebApi)
**Entities**: 26 entities across 5 modules (Sales, Inventory, Customer, Employee, Store)
**Endpoints**: ~50 REST endpoints + 2 SignalR hubs
**Testing Strategy**: TDD with unit, integration, and contract tests

## Task Notation

- **[P]**: Parallelizable (can be worked on simultaneously with other [P] tasks)
- **T###**: Sequential task number (001-170)
- **Dependencies**: Listed as `Depends on: T###`
- **File paths**: All absolute paths from repository root

---

## Phase A: Foundation Setup (T001-T015)

### T001 [P] Create solution structure and project scaffolding ✅
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\PointOfSale.sln`
**Purpose**: Initialize .NET 9 solution with 4 projects following Clean Architecture
**Dependencies**: None
**Acceptance**:
- Solution file created with 4 projects: Domain, Application, Infrastructure, WebApi
- All projects organized under `src/` folder following Clean Architecture
- Project references configured: WebApi → Infrastructure → Application → Domain
- All projects target .NET 9.0
- Class libraries use `<Nullable>enable</Nullable>`
- Directory structure includes: Entities/, Events/, Exceptions/, Interfaces/, ValueObjects/ in Domain
```bash
# Create solution
dotnet new sln -n PointOfSale

# Create src directory
mkdir src

# Create projects in src folder
dotnet new classlib -n Domain -o src/Domain -f net9.0
dotnet new classlib -n Application -o src/Application -f net9.0
dotnet new classlib -n Infrastructure -o src/Infrastructure -f net9.0
dotnet new webapi -n WebApi -o src/WebApi -f net9.0

# Add projects to solution
dotnet sln add src/Domain/Domain.csproj
dotnet sln add src/Application/Application.csproj
dotnet sln add src/Infrastructure/Infrastructure.csproj
dotnet sln add src/WebApi/WebApi.csproj

# Configure project references
dotnet add src/Application/Application.csproj reference src/Domain/Domain.csproj
dotnet add src/Infrastructure/Infrastructure.csproj reference src/Application/Application.csproj
dotnet add src/WebApi/WebApi.csproj reference src/Infrastructure/Infrastructure.csproj

# Create Domain layer structure
mkdir src/Domain/Entities/Sales
mkdir src/Domain/Entities/Inventory
mkdir src/Domain/Entities/Customers
mkdir src/Domain/Entities/Employees
mkdir src/Domain/Entities/Stores
mkdir src/Domain/Common
mkdir src/Domain/Events
mkdir src/Domain/Exceptions
mkdir src/Domain/Interfaces
mkdir src/Domain/ValueObjects
```

---

### T002 [P] Create BaseEntity and audit infrastructure ✅
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Common\BaseEntity.cs`
**Purpose**: Implement base entity class with audit fields and soft delete support
**Dependencies**: T001
**Acceptance**:
- BaseEntity abstract class with Id (Guid), CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
- IAuditable interface defined
- All fields properly typed (DateTime for timestamps, string for user IDs)

---

### T003 [P] Install core NuGet packages ✅
**File**: Multiple `.csproj` files
**Purpose**: Add all required dependencies to projects
**Dependencies**: T001
**Acceptance**:
- Domain: No external packages (pure C#)
- Application: MediatR (12.x), FluentValidation (11.x), AutoMapper (13.x)
- Infrastructure: Npgsql.EntityFrameworkCore.PostgreSQL (9.x), StackExchange.Redis (2.x), Hangfire (1.8.x), Serilog (4.x)
- WebApi: Microsoft.AspNetCore.SignalR (9.x), Swashbuckle.AspNetCore (7.x)
```bash
# Application
dotnet add Application/Application.csproj package MediatR
dotnet add Application/Application.csproj package FluentValidation
dotnet add Application/Application.csproj package AutoMapper

# Infrastructure
dotnet add Infrastructure/Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add Infrastructure/Infrastructure.csproj package StackExchange.Redis
dotnet add Infrastructure/Infrastructure.csproj package Hangfire.PostgreSql
dotnet add Infrastructure/Infrastructure.csproj package Serilog.AspNetCore

# WebApi
dotnet add WebApi/WebApi.csproj package Swashbuckle.AspNetCore
```

---

### T004 [P] Configure Serilog structured logging ✅
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\WebApi\Program.cs`
**Purpose**: Setup centralized logging with Serilog to console and file sinks
**Dependencies**: T001, T003
**Acceptance**:
- Serilog configured in Program.cs with enrichers (timestamp, environment, machine name)
- Console and rolling file sinks configured
- Log levels: Information for production, Debug for development
- Logs written to `logs/pos-api-.log` with daily rotation
- Structured logging captures request details, user context, performance metrics

---

### T005 Setup PostgreSQL connection and DbContext ✅
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\ApplicationDbContext.cs`
**Purpose**: Create EF Core DbContext with multi-tenancy and audit interceptors
**Dependencies**: T001, T002, T003
**Acceptance**:
- ApplicationDbContext class inheriting from DbContext
- IApplicationDbContext interface in Application layer
- Connection string configured in appsettings.json
- Audit interceptor implemented (auto-populate CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
- Soft delete query filter applied globally
- SaveChangesAsync override for audit field population
- Multi-tenancy query filter placeholder (StoreId filtering)

---

### T006 Setup Redis distributed cache ✅
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Services\Caching\RedisCacheService.cs`
**Purpose**: Implement Redis caching service with connection management
**Dependencies**: T001, T003
**Acceptance**:
- IRedisCacheService interface in Application/Common/Interfaces
- RedisCacheService implementation with ConnectionMultiplexer
- Methods: GetAsync<T>, SetAsync<T>, RemoveAsync, ExistsAsync
- Connection string in appsettings.json (localhost:6379 for development)
- Retry policy using Polly (3 retries with exponential backoff)
- JSON serialization for complex objects

---

### T007 [P] Setup health check endpoints ✅
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\WebApi\Program.cs`
**Purpose**: Configure health checks for database, Redis, and application status
**Dependencies**: T001, T003, T005, T006
**Acceptance**:
- Health check endpoint at `/health`
- Database health check (PostgreSQL connection)
- Redis health check (connection and ping)
- Response includes status (Healthy/Degraded/Unhealthy) and component details
- Integration with ASP.NET Core health checks UI

---

### T008 [P] Create Docker Compose for local development ✅
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\docker-compose.yml`
**Purpose**: Provide local development environment with PostgreSQL and Redis
**Dependencies**: None
**Acceptance**:
- PostgreSQL 16 service with volume mount for persistence
- Redis 7 service with volume mount
- pgAdmin service for database management
- Environment variables for connection strings
- All services on custom network `pos-network`
- README with docker-compose up/down instructions

---

### T009 Setup dependency injection configuration ✅
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\WebApi\Program.cs`
**Purpose**: Register all services and configure DI container
**Dependencies**: T001-T008
**Acceptance**:
- DependencyInjection.cs in each layer (Application, Infrastructure, WebApi)
- Application layer: MediatR, FluentValidation, AutoMapper registered
- Infrastructure layer: DbContext, Redis, external services registered
- WebApi layer: Controllers, SignalR, middleware registered
- Service lifetimes correctly configured (Scoped for DbContext, Singleton for Redis)

---

### T010 [P] Configure CORS and security headers ✅
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\WebApi\Program.cs`
**Purpose**: Setup CORS policy and security headers for API
**Dependencies**: T001
**Acceptance**:
- CORS policy allowing configured origins from appsettings.json
- Security headers middleware (X-Content-Type-Options, X-Frame-Options, X-XSS-Protection)
- HTTPS redirection enabled for production
- HSTS configured with 1-year max age

---

### T011 [P] Create value objects (Money, Address, PhoneNumber) ✅
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\ValueObjects\`
**Purpose**: Implement value objects for domain modeling
**Dependencies**: T001, T002
**Acceptance**:
- Money value object with Amount (decimal) and Currency (string)
- Address value object with Street, City, District, Ward fields
- PhoneNumber value object with validation for Vietnamese format (10 digits)
- All value objects immutable (record types)
- Equality based on value, not reference

---

### T012 [P] Create domain exceptions ✅
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Exceptions\`
**Purpose**: Define custom exceptions for domain violations
**Dependencies**: T001
**Acceptance**:
- DomainException base class
- Specific exceptions: EntityNotFoundException, InvalidStateTransitionException, BusinessRuleViolationException
- All exceptions include descriptive messages and error codes

---

### T013 [P] Setup MediatR pipeline behaviors ✅
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Common\Behaviours\`
**Purpose**: Implement cross-cutting concerns via MediatR pipeline
**Dependencies**: T001, T003
**Acceptance**:
- LoggingBehavior: Logs request/response for all commands and queries
- ValidationBehavior: Runs FluentValidation validators before handler execution
- PerformanceBehavior: Logs warnings for requests exceeding 3 seconds
- TransactionBehavior: Wraps commands in database transactions
- All behaviors registered in Application DependencyInjection

---

### T014 [P] Create common DTOs and response models ✅
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Common\Models\`
**Purpose**: Define shared DTOs and response wrappers
**Dependencies**: T001
**Acceptance**:
- Result<T> class for operation results (Success, Failure, Errors)
- PaginatedList<T> for paginated responses
- ValidationError record for validation failures
- ApiResponse<T> wrapper for all API responses

---

### T015 [P] Setup AutoMapper profiles ✅
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Common\Mappings\MappingProfile.cs`
**Purpose**: Configure object-to-object mapping
**Dependencies**: T001, T003
**Acceptance**:
- MappingProfile class inheriting from Profile
- Registered in Application DependencyInjection
- Placeholder mappings for common entities (will be populated in Phase B/C)

---

## Phase B: Domain Layer - Entities (T016-T041)

### T016 [P] Create Order entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Sales\Order.cs`
**Purpose**: Implement Order aggregate root with business logic
**Dependencies**: T002
**Acceptance**:
- Order entity with all fields from data-model.md
- OrderStatus enum (Draft, Completed, Voided, Returned, OnHold)
- OrderType enum (Sale, Return, Exchange)
- Navigation properties: OrderItems, Payments, Store, Customer, Cashier, Shift
- State transition methods: Complete(), Void(), Hold(), AddPayment()
- Domain events: OrderCreatedEvent, OrderCompletedEvent, OrderVoidedEvent
- Business rules: TotalAmount = Subtotal + TaxAmount - DiscountAmount
- Validation in constructor: OrderNumber required, Subtotal >= 0

---

### T017 [P] Create OrderItem entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Sales\OrderItem.cs`
**Purpose**: Implement OrderItem entity for order line items
**Dependencies**: T002, T016
**Acceptance**:
- OrderItem entity with fields from data-model.md
- Navigation properties: Order, ProductVariant
- Calculation method: CalculateLineTotal() = (Quantity * UnitPrice - DiscountAmount) + TaxAmount
- Validation: Quantity > 0, UnitPrice >= 0

---

### T018 [P] Create Payment entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Sales\Payment.cs`
**Purpose**: Implement Payment entity for payment transactions
**Dependencies**: T002
**Acceptance**:
- Payment entity with fields from data-model.md
- PaymentStatus enum (Pending, Completed, Failed, Refunded)
- Navigation properties: Order, PaymentMethod
- Methods: MarkAsCompleted(), MarkAsFailed(), Refund()
- Validation: Amount > 0, ReferenceNumber required for electronic payments

---

### T019 [P] Create Shift entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Sales\Shift.cs`
**Purpose**: Implement Shift entity for cashier work sessions
**Dependencies**: T002
**Acceptance**:
- Shift entity with fields from data-model.md
- ShiftStatus enum (Open, Closed)
- Navigation properties: Store, Cashier, Orders
- Methods: CloseShift(), CalculateCashDifference()
- Business rule: Only one open shift per cashier at a time
- Validation: EndTime required when Status = Closed

---

### T020 [P] Create PaymentMethod entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Sales\PaymentMethod.cs`
**Purpose**: Implement PaymentMethod configuration entity
**Dependencies**: T002
**Acceptance**:
- PaymentMethod entity with fields from data-model.md
- PaymentMethodType enum (Cash, Card, EWallet, BankTransfer, Other)
- Validation: Code unique, DisplayOrder >= 0

---

### T021 [P] Create Product entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Inventory\Product.cs`
**Purpose**: Implement Product aggregate root
**Dependencies**: T002
**Acceptance**:
- Product entity with fields from data-model.md
- ProductType enum (Single, Variable)
- Navigation properties: Category, Brand, Variants
- Validation: SKU unique, CostPrice/RetailPrice >= 0
- Business rule: ReorderLevel/ReorderQuantity required if TrackInventory = true

---

### T022 [P] Create ProductVariant entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Inventory\ProductVariant.cs`
**Purpose**: Implement ProductVariant for product variations
**Dependencies**: T002, T021
**Acceptance**:
- ProductVariant entity with fields from data-model.md
- Navigation properties: Product, InventoryLevels
- Attributes stored as JSON (size, color, etc.)
- Validation: SKU unique, Barcode unique if provided, only one default variant per product

---

### T023 [P] Create Category entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Inventory\Category.cs`
**Purpose**: Implement Category with hierarchical structure
**Dependencies**: T002
**Acceptance**:
- Category entity with fields from data-model.md
- Self-referential navigation: Parent, Children
- Validation: Name unique within same ParentId, no circular references

---

### T024 [P] Create Brand entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Inventory\Brand.cs`
**Purpose**: Implement Brand entity for product manufacturers
**Dependencies**: T002
**Acceptance**:
- Brand entity with fields from data-model.md
- Navigation properties: Products
- Validation: Name unique, Website must be valid URL if provided

---

### T025 [P] Create InventoryLevel entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Inventory\InventoryLevel.cs`
**Purpose**: Implement InventoryLevel for stock tracking
**Dependencies**: T002, T022
**Acceptance**:
- InventoryLevel entity with fields from data-model.md
- Navigation properties: ProductVariant, Store
- Business rule: OnHandQuantity = AvailableQuantity + ReservedQuantity
- Methods: ReserveStock(), ReleaseStock(), AdjustQuantity()
- Validation: Unique constraint on (ProductVariantId, StoreId)

---

### T026 [P] Create InventoryReceipt entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Inventory\InventoryReceipt.cs`
**Purpose**: Implement InventoryReceipt for goods receipt
**Dependencies**: T002
**Acceptance**:
- InventoryReceipt entity with fields from data-model.md
- ReceiptStatus enum (Draft, Completed, Cancelled)
- Navigation properties: Store, Supplier, Items
- State transition methods: Complete(), Cancel()
- Validation: ReceiptNumber unique, TotalAmount >= 0

---

### T027 [P] Create InventoryReceiptItem entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Inventory\InventoryReceiptItem.cs`
**Purpose**: Implement line items for inventory receipts
**Dependencies**: T002, T026
**Acceptance**:
- InventoryReceiptItem entity with fields from data-model.md
- Navigation properties: Receipt, ProductVariant
- Calculation: LineTotal = Quantity * UnitCost
- Validation: Quantity > 0, UnitCost >= 0

---

### T028 [P] Create InventoryIssue entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Inventory\InventoryIssue.cs`
**Purpose**: Implement InventoryIssue for adjustments and transfers
**Dependencies**: T002
**Acceptance**:
- InventoryIssue entity with fields from data-model.md
- IssueType enum (Adjustment, Damage, Loss, Transfer, Return)
- IssueStatus enum (Draft, Completed, Cancelled)
- Navigation properties: Store, DestinationStore, Items
- Validation: DestinationStoreId required when Type = Transfer

---

### T029 [P] Create InventoryIssueItem entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Inventory\InventoryIssueItem.cs`
**Purpose**: Implement line items for inventory issues
**Dependencies**: T002, T028
**Acceptance**:
- InventoryIssueItem entity with fields from data-model.md
- Navigation properties: Issue, ProductVariant
- Validation: Quantity > 0

---

### T030 [P] Create Stocktake entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Inventory\Stocktake.cs`
**Purpose**: Implement Stocktake for physical inventory counts
**Dependencies**: T002
**Acceptance**:
- Stocktake entity with fields from data-model.md
- StocktakeStatus enum (Scheduled, InProgress, Completed, Cancelled)
- Navigation properties: Store, Items
- State transitions: Start(), Complete(), Cancel()
- Validation: CompletedDate >= ScheduledDate

---

### T031 [P] Create StocktakeItem entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Inventory\StocktakeItem.cs`
**Purpose**: Implement counted items in stocktake
**Dependencies**: T002, T030
**Acceptance**:
- StocktakeItem entity with fields from data-model.md
- Navigation properties: Stocktake, ProductVariant
- Calculation: Variance = CountedQuantity - SystemQuantity
- Validation: Unique (StocktakeId, ProductVariantId)

---

### T032 [P] Create Customer entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Customers\Customer.cs`
**Purpose**: Implement Customer aggregate root
**Dependencies**: T002
**Acceptance**:
- Customer entity with fields from data-model.md
- CustomerGender enum (Male, Female, Other)
- Navigation properties: CustomerGroup, LoyaltyTransactions, Debts
- Methods: AddLoyaltyPoints(), RedeemPoints(), RecordPurchase()
- Validation: CustomerNumber unique, Phone or Email required, Phone must be valid Vietnamese format

---

### T033 [P] Create CustomerGroup entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Customers\CustomerGroup.cs`
**Purpose**: Implement CustomerGroup for segmentation
**Dependencies**: T002
**Acceptance**:
- CustomerGroup entity with fields from data-model.md
- Navigation properties: Customers
- Validation: Name unique, DiscountPercentage 0-100, LoyaltyPointsMultiplier >= 0

---

### T034 [P] Create LoyaltyTransaction entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Customers\LoyaltyTransaction.cs`
**Purpose**: Implement LoyaltyTransaction for points tracking
**Dependencies**: T002, T032
**Acceptance**:
- LoyaltyTransaction entity with fields from data-model.md
- LoyaltyTransactionType enum (Earned, Redeemed, Adjusted, Expired)
- Navigation properties: Customer, Order
- Validation: Points > 0 for Earned, < 0 for Redeemed/Expired

---

### T035 [P] Create Debt entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Customers\Debt.cs`
**Purpose**: Implement Debt for account receivables
**Dependencies**: T002, T032
**Acceptance**:
- Debt entity with fields from data-model.md
- DebtStatus enum (Pending, PartiallyPaid, Paid, Overdue, WrittenOff)
- Navigation properties: Customer, Order
- Methods: RecordPayment(), MarkOverdue(), WriteOff()
- Calculation: RemainingAmount = Amount - PaidAmount
- Validation: DebtNumber unique, Amount > 0

---

### T036 [P] Create User entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Employees\User.cs`
**Purpose**: Implement User entity for employees
**Dependencies**: T002
**Acceptance**:
- User entity with fields from data-model.md
- Navigation properties: Store, Role, Commissions
- Methods: UpdateRefreshToken(), MarkLastLogin()
- Validation: Username unique, Email unique if provided

---

### T037 [P] Create Role entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Employees\Role.cs`
**Purpose**: Implement Role with permissions
**Dependencies**: T002
**Acceptance**:
- Role entity with fields from data-model.md
- Navigation properties: Users, Permissions
- Validation: Name unique

---

### T038 [P] Create Permission entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Employees\Permission.cs`
**Purpose**: Implement Permission for granular access control
**Dependencies**: T002, T037
**Acceptance**:
- Permission entity with fields from data-model.md
- Navigation properties: Role
- Validation: Unique (RoleId, Resource, Action)

---

### T039 [P] Create Commission entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Employees\Commission.cs`
**Purpose**: Implement Commission for sales commissions
**Dependencies**: T002, T036
**Acceptance**:
- Commission entity with fields from data-model.md
- CommissionStatus enum (Pending, Approved, Paid, Cancelled)
- Navigation properties: User, Order
- Calculation: CommissionAmount = OrderAmount * (CommissionRate / 100)
- Validation: Unique (UserId, OrderId)

---

### T040 [P] Create Store entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Stores\Store.cs`
**Purpose**: Implement Store entity for locations
**Dependencies**: T002
**Acceptance**:
- Store entity with fields from data-model.md
- StoreType enum (RetailStore, Warehouse, Both)
- Validation: Code unique

---

### T041 [P] Create Supplier entity
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Domain\Entities\Stores\Supplier.cs`
**Purpose**: Implement Supplier entity
**Dependencies**: T002
**Acceptance**:
- Supplier entity with fields from data-model.md
- Validation: Code unique

---

## Phase C: Application Layer - Commands & Queries (T042-T095)

### T042 [P] Create LoginCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Auth\Commands\LoginCommand.cs`
**Purpose**: Implement user authentication
**Dependencies**: T001, T013, T036
**Acceptance**:
- LoginCommand with Username, Password properties
- LoginCommandHandler validates credentials, generates JWT token
- Returns access token and refresh token
- FluentValidation validator: Username and Password required
- AutoMapper profile for UserDto

---

### T043 [P] Create RefreshTokenCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Auth\Commands\RefreshTokenCommand.cs`
**Purpose**: Implement token refresh
**Dependencies**: T001, T013, T036, T042
**Acceptance**:
- RefreshTokenCommand with RefreshToken property
- Handler validates refresh token, generates new access token
- Updates refresh token in database
- Returns new token pair

---

### T044 [P] Create LogoutCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Auth\Commands\LogoutCommand.cs`
**Purpose**: Implement user logout
**Dependencies**: T001, T013, T036
**Acceptance**:
- LogoutCommand with UserId property
- Handler invalidates refresh token
- Clears Redis session cache

---

### T045 [P] Create StartShiftCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Auth\Commands\StartShiftCommand.cs`
**Purpose**: Implement shift start for cashiers
**Dependencies**: T001, T013, T019
**Acceptance**:
- StartShiftCommand with StoreId, CashierId, OpeningCash
- Handler creates new Shift with status Open
- Validates: Only one open shift per cashier
- Returns ShiftDto

---

### T046 [P] Create EndShiftCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Auth\Commands\EndShiftCommand.cs`
**Purpose**: Implement shift closure
**Dependencies**: T001, T013, T019, T045
**Acceptance**:
- EndShiftCommand with ShiftId, ClosingCash
- Handler updates Shift status to Closed
- Calculates CashDifference, TotalSales, TotalTransactions
- Returns ShiftSummaryDto

---

### T047 [P] Create CreateOrderCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\POS\Commands\CreateOrderCommand.cs`
**Purpose**: Implement order creation
**Dependencies**: T001, T013, T016, T017
**Acceptance**:
- CreateOrderCommand with StoreId, CustomerId, CashierId, ShiftId, Items[]
- Handler creates Order with OrderItems
- Generates OrderNumber (ORD-YYYYMMDD-XXXX)
- Calculates Subtotal, TaxAmount, DiscountAmount, TotalAmount
- Reserves inventory (decrements AvailableQuantity, increments ReservedQuantity)
- Returns OrderDto

---

### T048 [P] Create GetOrderQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\POS\Queries\GetOrderQuery.cs`
**Purpose**: Retrieve order details
**Dependencies**: T001, T013, T016
**Acceptance**:
- GetOrderQuery with OrderId property
- Handler retrieves Order with related entities (Items, Payments, Customer)
- Uses AsNoTracking for performance
- Returns OrderDto with full details

---

### T049 [P] Create AddPaymentCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\POS\Commands\AddPaymentCommand.cs`
**Purpose**: Add payment to order
**Dependencies**: T001, T013, T016, T018
**Acceptance**:
- AddPaymentCommand with OrderId, PaymentMethodId, Amount, ReferenceNumber
- Handler creates Payment record
- Validates: Total payments <= Order.TotalAmount
- Generates PaymentNumber (PAY-YYYYMMDD-XXXX)
- Returns PaymentDto

---

### T050 [P] Create HoldOrderCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\POS\Commands\HoldOrderCommand.cs`
**Purpose**: Put order on hold
**Dependencies**: T001, T013, T016
**Acceptance**:
- HoldOrderCommand with OrderId property
- Handler updates Order.Status to OnHold
- Keeps inventory reservation
- Returns OrderDto

---

### T051 [P] Create GetHeldOrdersQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\POS\Queries\GetHeldOrdersQuery.cs`
**Purpose**: Retrieve all held orders for a store
**Dependencies**: T001, T013, T016
**Acceptance**:
- GetHeldOrdersQuery with StoreId property
- Handler retrieves Orders with Status = OnHold
- Orders sorted by CreatedAt descending
- Returns List<OrderSummaryDto>

---

### T052 [P] Create CompleteOrderCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\POS\Commands\CompleteOrderCommand.cs`
**Purpose**: Complete order transaction
**Dependencies**: T001, T013, T016, T025, T034
**Acceptance**:
- CompleteOrderCommand with OrderId property
- Handler updates Order.Status to Completed
- Validates: Total payments >= TotalAmount
- Commits inventory (AvailableQuantity already decremented, ReservedQuantity to 0)
- Creates LoyaltyTransaction if customer present
- Publishes OrderCompletedEvent for SignalR notification
- Returns OrderDto

---

### T053 [P] Create ProcessReturnCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\POS\Commands\ProcessReturnCommand.cs`
**Purpose**: Process order return
**Dependencies**: T001, T013, T016, T025
**Acceptance**:
- ProcessReturnCommand with OrderId, ReturnItems[], Reason
- Handler creates new Order with Type = Return
- References original order
- Restores inventory (increments AvailableQuantity)
- Creates refund Payment record
- Returns OrderDto

---

### T054 [P] Create GenerateReceiptQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\POS\Queries\GenerateReceiptQuery.cs`
**Purpose**: Generate receipt for completed order
**Dependencies**: T001, T013, T016
**Acceptance**:
- GenerateReceiptQuery with OrderId property
- Handler retrieves Order with all details
- Formats receipt data (store info, items, payments, totals)
- Returns ReceiptDto (can be HTML, PDF, or JSON)

---

### T055 [P] Create SearchProductsQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\POS\Queries\SearchProductsQuery.cs`
**Purpose**: Search products for POS screen
**Dependencies**: T001, T013, T021, T022
**Acceptance**:
- SearchProductsQuery with SearchTerm, StoreId, CategoryId, PageNumber, PageSize
- Handler searches Products and ProductVariants by Name, SKU, Barcode
- Includes current InventoryLevel for specified StoreId
- Uses PostgreSQL full-text search for performance
- Returns PaginatedList<ProductSearchResultDto>

---

### T056 [P] Create GetProductsQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Inventory\Queries\GetProductsQuery.cs`
**Purpose**: Retrieve product list with filters
**Dependencies**: T001, T013, T021
**Acceptance**:
- GetProductsQuery with CategoryId, BrandId, IsActive, PageNumber, PageSize
- Handler retrieves Products with pagination
- Includes Category and Brand
- Returns PaginatedList<ProductDto>

---

### T057 [P] Create CreateProductCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Inventory\Commands\CreateProductCommand.cs`
**Purpose**: Create new product
**Dependencies**: T001, T013, T021, T022
**Acceptance**:
- CreateProductCommand with SKU, Name, Description, CategoryId, BrandId, etc.
- Handler creates Product and default ProductVariant
- Validates: SKU unique
- Returns ProductDto

---

### T058 [P] Create UpdateProductCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Inventory\Commands\UpdateProductCommand.cs`
**Purpose**: Update existing product
**Dependencies**: T001, T013, T021, T057
**Acceptance**:
- UpdateProductCommand with ProductId and updated fields
- Handler updates Product entity
- Validates: Entity exists
- Returns ProductDto

---

### T059 [P] Create CreateInventoryReceiptCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Inventory\Commands\CreateInventoryReceiptCommand.cs`
**Purpose**: Create goods receipt
**Dependencies**: T001, T013, T026, T027
**Acceptance**:
- CreateInventoryReceiptCommand with StoreId, SupplierId, Items[]
- Handler creates InventoryReceipt with status Draft
- Generates ReceiptNumber (GRN-YYYYMMDD-XXXX)
- Creates InventoryReceiptItems
- Returns InventoryReceiptDto

---

### T060 [P] Create CompleteInventoryReceiptCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Inventory\Commands\CompleteInventoryReceiptCommand.cs`
**Purpose**: Complete goods receipt and update inventory
**Dependencies**: T001, T013, T026, T025, T059
**Acceptance**:
- CompleteInventoryReceiptCommand with ReceiptId
- Handler updates InventoryReceipt status to Completed
- Increments InventoryLevel.OnHandQuantity and AvailableQuantity for each item
- Returns InventoryReceiptDto

---

### T061 [P] Create CreateInventoryIssueCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Inventory\Commands\CreateInventoryIssueCommand.cs`
**Purpose**: Create inventory adjustment or transfer
**Dependencies**: T001, T013, T028, T029, T025
**Acceptance**:
- CreateInventoryIssueCommand with StoreId, Type, DestinationStoreId, Items[], Reason
- Handler creates InventoryIssue with status Draft
- Generates IssueNumber (ISS-YYYYMMDD-XXXX)
- Validates: DestinationStoreId required if Type = Transfer
- Returns InventoryIssueDto

---

### T062 [P] Create CompleteInventoryIssueCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Inventory\Commands\CompleteInventoryIssueCommand.cs`
**Purpose**: Complete inventory issue and update stock
**Dependencies**: T001, T013, T028, T025, T061
**Acceptance**:
- CompleteInventoryIssueCommand with IssueId
- Handler updates InventoryIssue status to Completed
- Decrements source InventoryLevel (OnHandQuantity, AvailableQuantity)
- If Transfer: Increments destination InventoryLevel
- Returns InventoryIssueDto

---

### T063 [P] Create GetStockLevelsQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Inventory\Queries\GetStockLevelsQuery.cs`
**Purpose**: Retrieve current stock levels
**Dependencies**: T001, T013, T025
**Acceptance**:
- GetStockLevelsQuery with StoreId, ProductId, LowStock flag
- Handler retrieves InventoryLevels with Product/Variant details
- If LowStock = true: Filter where AvailableQuantity <= Product.ReorderLevel
- Returns List<InventoryLevelDto>

---

### T064 [P] Create CreateStocktakeCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Inventory\Commands\CreateStocktakeCommand.cs`
**Purpose**: Schedule stocktake
**Dependencies**: T001, T013, T030, T031
**Acceptance**:
- CreateStocktakeCommand with StoreId, ScheduledDate
- Handler creates Stocktake with status Scheduled
- Generates StocktakeNumber (STK-YYYYMMDD-XXXX)
- Creates StocktakeItems for all ProductVariants with current SystemQuantity
- Returns StocktakeDto

---

### T065 [P] Create FinalizeStocktakeCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Inventory\Commands\FinalizeStocktakeCommand.cs`
**Purpose**: Complete stocktake and adjust inventory
**Dependencies**: T001, T013, T030, T025, T064
**Acceptance**:
- FinalizeStocktakeCommand with StocktakeId
- Handler updates Stocktake status to Completed
- For each StocktakeItem with variance: Create InventoryIssue (Type = Adjustment)
- Updates InventoryLevel to match CountedQuantity
- Returns StocktakeDto with variance summary

---

### T066 [P] Create GenerateBarcodesCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Inventory\Commands\GenerateBarcodesCommand.cs`
**Purpose**: Generate barcodes for product variants
**Dependencies**: T001, T013, T022
**Acceptance**:
- GenerateBarcodesCommand with ProductVariantIds[]
- Handler generates unique barcodes (EAN-13 format)
- Updates ProductVariant.Barcode field
- Returns List<BarcodeDto> with variant details

---

### T067 [P] Create GetCustomersQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Customers\Queries\GetCustomersQuery.cs`
**Purpose**: Retrieve customer list
**Dependencies**: T001, T013, T032
**Acceptance**:
- GetCustomersQuery with CustomerGroupId, IsActive, PageNumber, PageSize
- Handler retrieves Customers with pagination
- Returns PaginatedList<CustomerDto>

---

### T068 [P] Create CreateCustomerCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Customers\Commands\CreateCustomerCommand.cs`
**Purpose**: Create new customer
**Dependencies**: T001, T013, T032
**Acceptance**:
- CreateCustomerCommand with Name, Phone, Email, Address, etc.
- Handler creates Customer
- Generates CustomerNumber (CUS-XXXXXXXX)
- Validates: Phone or Email required, unique constraints
- Returns CustomerDto

---

### T069 [P] Create SearchCustomersQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Customers\Queries\SearchCustomersQuery.cs`
**Purpose**: Search customers by phone, name, or email
**Dependencies**: T001, T013, T032
**Acceptance**:
- SearchCustomersQuery with SearchTerm property
- Handler searches by Phone, Name, Email using PostgreSQL full-text search
- Returns List<CustomerDto>

---

### T070 [P] Create GetCustomerOrdersQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Customers\Queries\GetCustomerOrdersQuery.cs`
**Purpose**: Retrieve customer purchase history
**Dependencies**: T001, T013, T016, T032
**Acceptance**:
- GetCustomerOrdersQuery with CustomerId, PageNumber, PageSize
- Handler retrieves Orders for customer
- Sorted by CreatedAt descending
- Returns PaginatedList<OrderSummaryDto>

---

### T071 [P] Create AddLoyaltyPointsCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Customers\Commands\AddLoyaltyPointsCommand.cs`
**Purpose**: Add loyalty points to customer
**Dependencies**: T001, T013, T032, T034
**Acceptance**:
- AddLoyaltyPointsCommand with CustomerId, Points, Type, Description, OrderId
- Handler creates LoyaltyTransaction
- Updates Customer.LoyaltyPoints
- Returns LoyaltyTransactionDto

---

### T072 [P] Create GetLoyaltyPointsHistoryQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Customers\Queries\GetLoyaltyPointsHistoryQuery.cs`
**Purpose**: Retrieve loyalty points history
**Dependencies**: T001, T013, T034
**Acceptance**:
- GetLoyaltyPointsHistoryQuery with CustomerId, PageNumber, PageSize
- Handler retrieves LoyaltyTransactions
- Returns PaginatedList<LoyaltyTransactionDto>

---

### T073 [P] Create CreateDebtCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Customers\Commands\CreateDebtCommand.cs`
**Purpose**: Create customer debt record
**Dependencies**: T001, T013, T035
**Acceptance**:
- CreateDebtCommand with CustomerId, OrderId, Amount, DueDate
- Handler creates Debt with status Pending
- Generates DebtNumber (DBT-YYYYMMDD-XXXX)
- Returns DebtDto

---

### T074 [P] Create RecordDebtPaymentCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Customers\Commands\RecordDebtPaymentCommand.cs`
**Purpose**: Record payment towards debt
**Dependencies**: T001, T013, T035, T073
**Acceptance**:
- RecordDebtPaymentCommand with DebtId, PaymentAmount
- Handler updates Debt.PaidAmount and RemainingAmount
- Updates Status (PartiallyPaid if RemainingAmount > 0, Paid if RemainingAmount = 0)
- Returns DebtDto

---

### T075 [P] Create GetCustomerGroupsQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Customers\Queries\GetCustomerGroupsQuery.cs`
**Purpose**: Retrieve customer groups
**Dependencies**: T001, T013, T033
**Acceptance**:
- GetCustomerGroupsQuery with IsActive filter
- Handler retrieves CustomerGroups
- Returns List<CustomerGroupDto>

---

### T076 [P] Create GetEmployeesQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Employees\Queries\GetEmployeesQuery.cs`
**Purpose**: Retrieve employee list
**Dependencies**: T001, T013, T036
**Acceptance**:
- GetEmployeesQuery with StoreId, RoleId, IsActive, PageNumber, PageSize
- Handler retrieves Users with Role and Store details
- Returns PaginatedList<EmployeeDto>

---

### T077 [P] Create CreateEmployeeCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Employees\Commands\CreateEmployeeCommand.cs`
**Purpose**: Create new employee
**Dependencies**: T001, T013, T036
**Acceptance**:
- CreateEmployeeCommand with Username, Password, FullName, Email, Phone, StoreId, RoleId
- Handler creates User with hashed password (BCrypt)
- Validates: Username unique, Email unique
- Returns EmployeeDto

---

### T078 [P] Create GetRolesQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Employees\Queries\GetRolesQuery.cs`
**Purpose**: Retrieve roles with permissions
**Dependencies**: T001, T013, T037
**Acceptance**:
- GetRolesQuery with IsActive filter
- Handler retrieves Roles with Permissions
- Returns List<RoleDto>

---

### T079 [P] Create CreateRoleCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Employees\Commands\CreateRoleCommand.cs`
**Purpose**: Create new role with permissions
**Dependencies**: T001, T013, T037, T038
**Acceptance**:
- CreateRoleCommand with Name, Description, PermissionIds[]
- Handler creates Role and associated Permissions
- Validates: Name unique
- Returns RoleDto

---

### T080 [P] Create GetPermissionsQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Employees\Queries\GetPermissionsQuery.cs`
**Purpose**: Retrieve all available permissions
**Dependencies**: T001, T013, T038
**Acceptance**:
- GetPermissionsQuery (no filters)
- Handler retrieves all Permissions grouped by Resource
- Returns List<PermissionDto>

---

### T081 [P] Create GetCommissionsQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Employees\Queries\GetCommissionsQuery.cs`
**Purpose**: Retrieve employee commission records
**Dependencies**: T001, T013, T039
**Acceptance**:
- GetCommissionsQuery with UserId, Status, DateFrom, DateTo, PageNumber, PageSize
- Handler retrieves Commissions with Order details
- Returns PaginatedList<CommissionDto>

---

### T082 [P] Create SalesReportQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Reports\Queries\SalesReportQuery.cs`
**Purpose**: Generate sales report
**Dependencies**: T001, T013, T016
**Acceptance**:
- SalesReportQuery with StoreId, DateFrom, DateTo, GroupBy (Day/Week/Month)
- Handler aggregates Orders by period
- Calculates: TotalSales, TotalOrders, AverageOrderValue, TotalTax, TotalDiscount
- Returns SalesReportDto with time-series data

---

### T083 [P] Create TopProductsReportQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Reports\Queries\TopProductsReportQuery.cs`
**Purpose**: Generate top-selling products report
**Dependencies**: T001, T013, T016, T017
**Acceptance**:
- TopProductsReportQuery with StoreId, DateFrom, DateTo, TopN (default 20)
- Handler aggregates OrderItems by Product
- Calculates: TotalQuantity, TotalRevenue, OrderCount
- Sorted by TotalRevenue descending
- Returns List<TopProductDto>

---

### T084 [P] Create InventoryMovementReportQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Reports\Queries\InventoryMovementReportQuery.cs`
**Purpose**: Generate inventory movement report
**Dependencies**: T001, T013, T026, T028, T016
**Acceptance**:
- InventoryMovementReportQuery with StoreId, ProductId, DateFrom, DateTo
- Handler aggregates inventory transactions (Receipts, Issues, Sales)
- Calculates: OpeningStock, Receipts, Issues, Sales, ClosingStock
- Returns InventoryMovementReportDto

---

### T085 [P] Create FinancialReportQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Reports\Queries\FinancialReportQuery.cs`
**Purpose**: Generate financial summary report
**Dependencies**: T001, T013, T016, T018
**Acceptance**:
- FinancialReportQuery with StoreId, DateFrom, DateTo
- Handler aggregates Orders and Payments
- Calculates: TotalRevenue, TotalCash, TotalCard, TotalEWallet, TotalRefunds, NetRevenue
- Breaks down by PaymentMethod
- Returns FinancialReportDto

---

### T086 [P] Create DashboardQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Reports\Queries\DashboardQuery.cs`
**Purpose**: Generate real-time dashboard metrics
**Dependencies**: T001, T013, T016, T025, T032
**Acceptance**:
- DashboardQuery with StoreId
- Handler retrieves today's metrics: TodaySales, TodayOrders, ActiveCustomers, LowStockCount
- Caches result for 5 minutes in Redis
- Returns DashboardDto

---

### T087 [P] Create CreateVNPayQRCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Integrations\Commands\CreateVNPayQRCommand.cs`
**Purpose**: Generate VNPay QR code for payment
**Dependencies**: T001, T013
**Acceptance**:
- CreateVNPayQRCommand with OrderId, Amount
- Handler calls VNPay API to generate QR code
- Stores transaction reference
- Returns QR code URL and transaction ID

---

### T088 [P] Create CalculateShippingFeeCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Integrations\Commands\CalculateShippingFeeCommand.cs`
**Purpose**: Calculate shipping fee via GHN/GHTK
**Dependencies**: T001, T013
**Acceptance**:
- CalculateShippingFeeCommand with FromAddress, ToAddress, Weight, Provider (GHN/GHTK)
- Handler calls provider API
- Returns shipping fee and estimated delivery time

---

### T089 [P] Create CreateShippingOrderCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Integrations\Commands\CreateShippingOrderCommand.cs`
**Purpose**: Create shipping order with logistics provider
**Dependencies**: T001, T013, T016
**Acceptance**:
- CreateShippingOrderCommand with OrderId, Provider, RecipientAddress
- Handler calls provider API to create shipping order
- Stores tracking number
- Returns ShippingOrderDto

---

### T090 [P] Create TrackShippingQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Integrations\Queries\TrackShippingQuery.cs`
**Purpose**: Track shipping order status
**Dependencies**: T001, T013, T089
**Acceptance**:
- TrackShippingQuery with TrackingNumber, Provider
- Handler calls provider API to get status
- Returns ShippingStatusDto

---

### T091 [P] Create HandleOrderStatusWebhookCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Integrations\Commands\HandleOrderStatusWebhookCommand.cs`
**Purpose**: Handle shipping status webhook
**Dependencies**: T001, T013, T089
**Acceptance**:
- HandleOrderStatusWebhookCommand with webhook payload
- Handler validates webhook signature
- Updates Order shipping status
- Publishes domain event for notifications

---

### T092 [P] Create GetStoresQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Stores\Queries\GetStoresQuery.cs`
**Purpose**: Retrieve store list
**Dependencies**: T001, T013, T040
**Acceptance**:
- GetStoresQuery with IsActive filter
- Handler retrieves Stores
- Returns List<StoreDto>

---

### T093 [P] Create CreateStoreCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Stores\Commands\CreateStoreCommand.cs`
**Purpose**: Create new store
**Dependencies**: T001, T013, T040
**Acceptance**:
- CreateStoreCommand with Code, Name, Type, Address, etc.
- Handler creates Store
- Validates: Code unique
- Returns StoreDto

---

### T094 [P] Create GetSuppliersQuery and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Stores\Queries\GetSuppliersQuery.cs`
**Purpose**: Retrieve supplier list
**Dependencies**: T001, T013, T041
**Acceptance**:
- GetSuppliersQuery with IsActive filter
- Handler retrieves Suppliers
- Returns List<SupplierDto>

---

### T095 [P] Create CreateSupplierCommand and handler
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Application\Features\Stores\Commands\CreateSupplierCommand.cs`
**Purpose**: Create new supplier
**Dependencies**: T001, T013, T041
**Acceptance**:
- CreateSupplierCommand with Code, Name, ContactPerson, Phone, Email, etc.
- Handler creates Supplier
- Validates: Code unique
- Returns SupplierDto

---

## Phase D: Infrastructure Layer - Persistence (T096-T125)

### T096 [P] Create OrderConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\OrderConfiguration.cs`
**Purpose**: Configure Order entity for EF Core
**Dependencies**: T005, T016
**Acceptance**:
- Implements IEntityTypeConfiguration<Order>
- All properties configured with correct types and constraints (from data-model.md)
- Relationships configured (OrderItems, Payments, Store, Customer, Cashier, Shift)
- Indexes created: OrderNumber (unique), (StoreId, CreatedAt), CustomerId, ShiftId, Status
- Decimal precision: 18,2 for money fields
- Table partitioning by CreatedAt (yearly partitions)

---

### T097 [P] Create OrderItemConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\OrderItemConfiguration.cs`
**Purpose**: Configure OrderItem entity
**Dependencies**: T005, T017
**Acceptance**:
- IEntityTypeConfiguration<OrderItem>
- Cascade delete with Order
- Indexes: OrderId, ProductVariantId, (OrderId, LineNumber) unique

---

### T098 [P] Create PaymentConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\PaymentConfiguration.cs`
**Purpose**: Configure Payment entity
**Dependencies**: T005, T018
**Acceptance**:
- IEntityTypeConfiguration<Payment>
- Indexes: PaymentNumber unique, OrderId, Status, ReferenceNumber

---

### T099 [P] Create ShiftConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\ShiftConfiguration.cs`
**Purpose**: Configure Shift entity
**Dependencies**: T005, T019
**Acceptance**:
- IEntityTypeConfiguration<Shift>
- Indexes: ShiftNumber unique, (CashierId, Status), (StoreId, StartTime)

---

### T100 [P] Create PaymentMethodConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\PaymentMethodConfiguration.cs`
**Purpose**: Configure PaymentMethod entity
**Dependencies**: T005, T020
**Acceptance**:
- IEntityTypeConfiguration<PaymentMethod>
- Configuration field as JSONB (PostgreSQL)
- Indexes: Code unique, IsActive

---

### T101 [P] Create ProductConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\ProductConfiguration.cs`
**Purpose**: Configure Product entity
**Dependencies**: T005, T021
**Acceptance**:
- IEntityTypeConfiguration<Product>
- Indexes: SKU unique, Name, CategoryId, BrandId, IsActive
- Full-text search index on Name (PostgreSQL GIN)

---

### T102 [P] Create ProductVariantConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\ProductVariantConfiguration.cs`
**Purpose**: Configure ProductVariant entity
**Dependencies**: T005, T022
**Acceptance**:
- IEntityTypeConfiguration<ProductVariant>
- Attributes field as JSONB
- Indexes: SKU unique, Barcode unique (with filter), ProductId, IsActive

---

### T103 [P] Create CategoryConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\CategoryConfiguration.cs`
**Purpose**: Configure Category entity
**Dependencies**: T005, T023
**Acceptance**:
- IEntityTypeConfiguration<Category>
- Self-referential relationship (Parent/Children)
- Indexes: (ParentId, Name) unique, IsActive

---

### T104 [P] Create BrandConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\BrandConfiguration.cs`
**Purpose**: Configure Brand entity
**Dependencies**: T005, T024
**Acceptance**:
- IEntityTypeConfiguration<Brand>
- Indexes: Name unique, IsActive

---

### T105 [P] Create InventoryLevelConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\InventoryLevelConfiguration.cs`
**Purpose**: Configure InventoryLevel entity
**Dependencies**: T005, T025
**Acceptance**:
- IEntityTypeConfiguration<InventoryLevel>
- Indexes: (ProductVariantId, StoreId) unique, StoreId

---

### T106 [P] Create InventoryReceiptConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\InventoryReceiptConfiguration.cs`
**Purpose**: Configure InventoryReceipt entity
**Dependencies**: T005, T026
**Acceptance**:
- IEntityTypeConfiguration<InventoryReceipt>
- Indexes: ReceiptNumber unique, (StoreId, ReceiptDate), SupplierId, Status

---

### T107 [P] Create InventoryReceiptItemConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\InventoryReceiptItemConfiguration.cs`
**Purpose**: Configure InventoryReceiptItem entity
**Dependencies**: T005, T027
**Acceptance**:
- IEntityTypeConfiguration<InventoryReceiptItem>
- Cascade delete with Receipt
- Indexes: ReceiptId, (ReceiptId, LineNumber) unique

---

### T108 [P] Create InventoryIssueConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\InventoryIssueConfiguration.cs`
**Purpose**: Configure InventoryIssue entity
**Dependencies**: T005, T028
**Acceptance**:
- IEntityTypeConfiguration<InventoryIssue>
- Indexes: IssueNumber unique, (StoreId, IssueDate), Type, Status

---

### T109 [P] Create InventoryIssueItemConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\InventoryIssueItemConfiguration.cs`
**Purpose**: Configure InventoryIssueItem entity
**Dependencies**: T005, T029
**Acceptance**:
- IEntityTypeConfiguration<InventoryIssueItem>
- Cascade delete with Issue
- Indexes: IssueId, (IssueId, LineNumber) unique

---

### T110 [P] Create StocktakeConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\StocktakeConfiguration.cs`
**Purpose**: Configure Stocktake entity
**Dependencies**: T005, T030
**Acceptance**:
- IEntityTypeConfiguration<Stocktake>
- Indexes: StocktakeNumber unique, (StoreId, ScheduledDate), Status

---

### T111 [P] Create StocktakeItemConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\StocktakeItemConfiguration.cs`
**Purpose**: Configure StocktakeItem entity
**Dependencies**: T005, T031
**Acceptance**:
- IEntityTypeConfiguration<StocktakeItem>
- Indexes: (StocktakeId, ProductVariantId) unique

---

### T112 [P] Create CustomerConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\CustomerConfiguration.cs`
**Purpose**: Configure Customer entity
**Dependencies**: T005, T032
**Acceptance**:
- IEntityTypeConfiguration<Customer>
- Indexes: CustomerNumber unique, Phone, Email, Name, CustomerGroupId, IsActive
- Full-text search index on Name

---

### T113 [P] Create CustomerGroupConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\CustomerGroupConfiguration.cs`
**Purpose**: Configure CustomerGroup entity
**Dependencies**: T005, T033
**Acceptance**:
- IEntityTypeConfiguration<CustomerGroup>
- Indexes: Name unique, IsActive

---

### T114 [P] Create LoyaltyTransactionConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\LoyaltyTransactionConfiguration.cs`
**Purpose**: Configure LoyaltyTransaction entity
**Dependencies**: T005, T034
**Acceptance**:
- IEntityTypeConfiguration<LoyaltyTransaction>
- Indexes: CustomerId, OrderId, (CustomerId, TransactionDate)

---

### T115 [P] Create DebtConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\DebtConfiguration.cs`
**Purpose**: Configure Debt entity
**Dependencies**: T005, T035
**Acceptance**:
- IEntityTypeConfiguration<Debt>
- Indexes: DebtNumber unique, CustomerId, (Status, DueDate)

---

### T116 [P] Create UserConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\UserConfiguration.cs`
**Purpose**: Configure User entity
**Dependencies**: T005, T036
**Acceptance**:
- IEntityTypeConfiguration<User>
- Indexes: Username unique, Email unique (with filter), StoreId, IsActive

---

### T117 [P] Create RoleConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\RoleConfiguration.cs`
**Purpose**: Configure Role entity
**Dependencies**: T005, T037
**Acceptance**:
- IEntityTypeConfiguration<Role>
- Indexes: Name unique, IsActive

---

### T118 [P] Create PermissionConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\PermissionConfiguration.cs`
**Purpose**: Configure Permission entity
**Dependencies**: T005, T038
**Acceptance**:
- IEntityTypeConfiguration<Permission>
- Cascade delete with Role
- Indexes: (RoleId, Resource, Action) unique

---

### T119 [P] Create CommissionConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\CommissionConfiguration.cs`
**Purpose**: Configure Commission entity
**Dependencies**: T005, T039
**Acceptance**:
- IEntityTypeConfiguration<Commission>
- Indexes: (UserId, OrderId) unique, (UserId, Status), CalculatedDate

---

### T120 [P] Create StoreConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\StoreConfiguration.cs`
**Purpose**: Configure Store entity
**Dependencies**: T005, T040
**Acceptance**:
- IEntityTypeConfiguration<Store>
- Indexes: Code unique, IsActive

---

### T121 [P] Create SupplierConfiguration (EF Core)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Configurations\SupplierConfiguration.cs`
**Purpose**: Configure Supplier entity
**Dependencies**: T005, T041
**Acceptance**:
- IEntityTypeConfiguration<Supplier>
- Indexes: Code unique, Name, IsActive

---

### T122 Create initial database migration
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\Migrations\`
**Purpose**: Generate initial EF Core migration with all entities
**Dependencies**: T005, T096-T121
**Acceptance**:
- Migration created with all 26 entities
- All indexes, constraints, relationships included
- Partition setup for Order table (yearly partitions)
- JSONB columns for PostgreSQL
- Full-text search indexes
- Migration applies successfully to PostgreSQL 16
```bash
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project WebApi
dotnet ef database update --project Infrastructure --startup-project WebApi
```

---

### T123 [P] Create database seed data
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Persistence\ApplicationDbContextSeed.cs`
**Purpose**: Seed initial data for development and testing
**Dependencies**: T005, T122
**Acceptance**:
- Seed data for PaymentMethods (Cash, Card, VNPay, MoMo)
- Seed default Roles (Admin, Manager, Cashier) with Permissions
- Seed default admin User (username: admin, password: Admin@123)
- Seed 2 sample Stores
- Seed 5 sample Categories
- Seed 3 sample Brands
- Seed 10 sample Products with variants
- Seed method called in Program.cs during startup (development only)

---

### T124 [P] Implement JWT token service
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Identity\JwtTokenService.cs`
**Purpose**: Generate and validate JWT tokens
**Dependencies**: T001, T003, T036
**Acceptance**:
- IJwtTokenService interface in Application layer
- JwtTokenService implementation
- Methods: GenerateAccessToken(), GenerateRefreshToken(), ValidateToken()
- Claims include: UserId, Username, RoleId, StoreId
- Access token expires in 15 minutes
- Refresh token expires in 7 days
- JWT settings in appsettings.json (Secret, Issuer, Audience)

---

### T125 [P] Implement password hashing service
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Identity\PasswordHashService.cs`
**Purpose**: Hash and verify passwords
**Dependencies**: T001, T003
**Acceptance**:
- IPasswordHashService interface in Application layer
- PasswordHashService implementation using BCrypt
- Methods: HashPassword(), VerifyPassword()
- Work factor: 12 (bcrypt rounds)

---

## Phase E: Infrastructure Layer - External Services (T126-T133)

### T126 [P] Implement VNPay payment client
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Services\External\VNPayClient.cs`
**Purpose**: Integrate with VNPay payment gateway
**Dependencies**: T001, T003
**Acceptance**:
- IVNPayClient interface in Application layer
- Methods: CreateQRPayment(), VerifyPaymentCallback()
- Handles HMAC signature generation and validation
- Configuration in appsettings.json (TmnCode, HashSecret, ApiUrl)
- Retry policy with Polly (3 retries, exponential backoff)

---

### T127 [P] Implement GHN shipping client
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Services\External\GHNClient.cs`
**Purpose**: Integrate with Giao Hang Nhanh (GHN) shipping
**Dependencies**: T001, T003
**Acceptance**:
- IGHNClient interface in Application layer
- Methods: CalculateFee(), CreateOrder(), TrackOrder()
- HTTP client with API token authentication
- Configuration in appsettings.json (ApiToken, ShopId, ApiUrl)
- Error handling for API failures

---

### T128 [P] Implement GHTK shipping client
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Services\External\GHTKClient.cs`
**Purpose**: Integrate with Giao Hang Tiet Kiem (GHTK) shipping
**Dependencies**: T001, T003
**Acceptance**:
- IGHTKClient interface in Application layer
- Methods: CalculateFee(), CreateOrder(), TrackOrder()
- HTTP client with API token authentication
- Configuration in appsettings.json (ApiToken, ApiUrl)

---

### T129 [P] Implement Hangfire background job service
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Services\Jobs\BackgroundJobService.cs`
**Purpose**: Setup background job processing with Hangfire
**Dependencies**: T001, T003, T005
**Acceptance**:
- Hangfire configured with PostgreSQL storage
- Dashboard enabled at /hangfire (admin only)
- Recurring jobs configured: MarkOverdueDebts (daily), ExpireLoyaltyPoints (weekly)
- Job retry policy: 3 attempts with exponential backoff

---

### T130 [P] Implement SignalR notification service
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Services\Notifications\SignalRNotificationService.cs`
**Purpose**: Send real-time notifications via SignalR
**Dependencies**: T001, T003
**Acceptance**:
- INotificationService interface in Application layer
- SignalRNotificationService implementation
- Methods: NotifyStoreAsync(), NotifyUserAsync(), NotifyAllAsync()
- Uses IHubContext<NotificationHub>
- Group-based messaging (per store)

---

### T131 [P] Implement email service
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Services\Email\EmailService.cs`
**Purpose**: Send emails for notifications and receipts
**Dependencies**: T001, T003
**Acceptance**:
- IEmailService interface in Application layer
- EmailService implementation using SMTP
- Methods: SendAsync(), SendReceiptAsync()
- Email templates in Razor
- Configuration in appsettings.json (SmtpHost, SmtpPort, FromEmail, Username, Password)

---

### T132 [P] Setup Vietnamese localization
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Localization\Resources\`
**Purpose**: Implement Vietnamese language support
**Dependencies**: T001
**Acceptance**:
- Resource files: SharedResources.vi.resx, ValidationMessages.vi.resx
- Request culture middleware configured
- Accept-Language header support
- Default culture: vi-VN
- Fallback culture: en-US

---

### T133 [P] Implement current user service
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\Infrastructure\Services\CurrentUserService.cs`
**Purpose**: Provide access to current authenticated user context
**Dependencies**: T001, T003
**Acceptance**:
- ICurrentUserService interface in Application layer
- CurrentUserService implementation
- Properties: UserId, Username, RoleId, StoreId, IsAuthenticated
- Reads from HttpContext.User claims
- Used in audit interceptor and query filters

---

## Phase F: Presentation Layer - API (T134-T145)

### T134 [P] Create AuthController
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\WebApi\Controllers\AuthController.cs`
**Purpose**: Expose authentication endpoints
**Dependencies**: T001, T042-T046
**Acceptance**:
- POST /api/auth/login → LoginCommand
- POST /api/auth/refresh → RefreshTokenCommand
- POST /api/auth/logout → LogoutCommand
- POST /api/auth/start-shift → StartShiftCommand
- POST /api/auth/end-shift → EndShiftCommand
- Returns ApiResponse<T> wrapper
- Swagger documentation with examples

---

### T135 [P] Create POSController
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\WebApi\Controllers\POSController.cs`
**Purpose**: Expose POS operation endpoints
**Dependencies**: T001, T047-T055
**Acceptance**:
- POST /api/pos/orders → CreateOrderCommand
- GET /api/pos/orders/{id} → GetOrderQuery
- POST /api/pos/orders/{id}/payments → AddPaymentCommand
- POST /api/pos/orders/{id}/hold → HoldOrderCommand
- GET /api/pos/orders/held → GetHeldOrdersQuery
- POST /api/pos/orders/{id}/complete → CompleteOrderCommand
- POST /api/pos/orders/{id}/return → ProcessReturnCommand
- GET /api/pos/orders/{id}/receipt → GenerateReceiptQuery
- GET /api/pos/products/search → SearchProductsQuery
- Authorization: [Authorize(Roles = "Cashier,Manager,Admin")]

---

### T136 [P] Create InventoryController
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\WebApi\Controllers\InventoryController.cs`
**Purpose**: Expose inventory management endpoints
**Dependencies**: T001, T056-T066
**Acceptance**:
- GET /api/inventory/products → GetProductsQuery
- POST /api/inventory/products → CreateProductCommand
- PUT /api/inventory/products/{id} → UpdateProductCommand
- POST /api/inventory/receipts → CreateInventoryReceiptCommand
- POST /api/inventory/receipts/{id}/complete → CompleteInventoryReceiptCommand
- POST /api/inventory/issues → CreateInventoryIssueCommand
- POST /api/inventory/issues/{id}/complete → CompleteInventoryIssueCommand
- GET /api/inventory/stock-levels → GetStockLevelsQuery
- POST /api/inventory/stocktakes → CreateStocktakeCommand
- POST /api/inventory/stocktakes/{id}/finalize → FinalizeStocktakeCommand
- POST /api/inventory/barcodes/generate → GenerateBarcodesCommand
- Authorization: [Authorize(Roles = "Manager,Admin")]

---

### T137 [P] Create CustomersController
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\WebApi\Controllers\CustomersController.cs`
**Purpose**: Expose customer management endpoints
**Dependencies**: T001, T067-T075
**Acceptance**:
- GET /api/customers → GetCustomersQuery
- POST /api/customers → CreateCustomerCommand
- GET /api/customers/search → SearchCustomersQuery
- GET /api/customers/{id}/orders → GetCustomerOrdersQuery
- POST /api/customers/{id}/loyalty-points → AddLoyaltyPointsCommand
- GET /api/customers/{id}/loyalty-points/history → GetLoyaltyPointsHistoryQuery
- POST /api/customers/{id}/debts → CreateDebtCommand
- POST /api/debts/{id}/payments → RecordDebtPaymentCommand
- GET /api/customer-groups → GetCustomerGroupsQuery
- Authorization: [Authorize(Roles = "Cashier,Manager,Admin")]

---

### T138 [P] Create EmployeesController
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\WebApi\Controllers\EmployeesController.cs`
**Purpose**: Expose employee management endpoints
**Dependencies**: T001, T076-T081
**Acceptance**:
- GET /api/employees → GetEmployeesQuery
- POST /api/employees → CreateEmployeeCommand
- GET /api/roles → GetRolesQuery
- POST /api/roles → CreateRoleCommand
- GET /api/permissions → GetPermissionsQuery
- GET /api/employees/{id}/commissions → GetCommissionsQuery
- Authorization: [Authorize(Roles = "Admin")]

---

### T139 [P] Create ReportsController
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\WebApi\Controllers\ReportsController.cs`
**Purpose**: Expose reporting endpoints
**Dependencies**: T001, T082-T086
**Acceptance**:
- GET /api/reports/sales → SalesReportQuery
- GET /api/reports/top-products → TopProductsReportQuery
- GET /api/reports/inventory-movement → InventoryMovementReportQuery
- GET /api/reports/financial → FinancialReportQuery
- GET /api/reports/dashboard → DashboardQuery
- Authorization: [Authorize(Roles = "Manager,Admin")]
- Response caching for reports (5 minutes)

---

### T140 [P] Create IntegrationsController
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\WebApi\Controllers\IntegrationsController.cs`
**Purpose**: Expose external integration endpoints
**Dependencies**: T001, T087-T091
**Acceptance**:
- POST /api/integrations/vnpay/qr → CreateVNPayQRCommand
- POST /api/integrations/shipping/calculate-fee → CalculateShippingFeeCommand
- POST /api/integrations/shipping/create-order → CreateShippingOrderCommand
- GET /api/integrations/shipping/track → TrackShippingQuery
- POST /api/webhooks/shipping-status → HandleOrderStatusWebhookCommand (no auth)
- Webhook endpoints validate signature

---

### T141 [P] Create StoresController
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\WebApi\Controllers\StoresController.cs`
**Purpose**: Expose store and supplier management endpoints
**Dependencies**: T001, T092-T095
**Acceptance**:
- GET /api/stores → GetStoresQuery
- POST /api/stores → CreateStoreCommand
- GET /api/suppliers → GetSuppliersQuery
- POST /api/suppliers → CreateSupplierCommand
- Authorization: [Authorize(Roles = "Admin")]

---

### T142 [P] Create NotificationHub (SignalR)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\WebApi\Hubs\NotificationHub.cs`
**Purpose**: Real-time notifications hub
**Dependencies**: T001, T003
**Acceptance**:
- Hub methods: JoinStore(storeId), LeaveStore(storeId)
- Server-to-client methods: OrderCreated, LowStockAlert, ShiftEnded
- OnConnectedAsync: Auto-join user's store group
- Authorization: [Authorize]

---

### T143 [P] Create DashboardHub (SignalR)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\WebApi\Hubs\DashboardHub.cs`
**Purpose**: Real-time dashboard metrics hub
**Dependencies**: T001, T003, T086
**Acceptance**:
- Hub methods: SubscribeToDashboard(storeId), UnsubscribeFromDashboard(storeId)
- Server-to-client methods: DashboardUpdated(dashboardDto)
- Pushes updates every 30 seconds
- Authorization: [Authorize(Roles = "Manager,Admin")]

---

### T144 [P] Create ExceptionHandlingMiddleware
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\WebApi\Middleware\ExceptionHandlingMiddleware.cs`
**Purpose**: Global exception handling and error responses
**Dependencies**: T001, T004
**Acceptance**:
- Catches all unhandled exceptions
- Logs exception details with Serilog
- Returns standardized error response (ProblemDetails)
- Maps exception types to HTTP status codes:
  - EntityNotFoundException → 404
  - ValidationException → 400
  - BusinessRuleViolationException → 422
  - Others → 500
- Includes correlation ID for tracking

---

### T145 [P] Create RateLimitingMiddleware
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\WebApi\Middleware\RateLimitingMiddleware.cs`
**Purpose**: Implement rate limiting per user/IP
**Dependencies**: T001, T006
**Acceptance**:
- Uses Redis for distributed rate limiting
- Limits: 100 requests per minute per user
- Returns 429 Too Many Requests when exceeded
- Includes Retry-After header
- Configurable in appsettings.json

---

## Phase G: Testing (T146-T162)

### T146 [P] Setup test projects
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\tests\`
**Purpose**: Create test project structure
**Dependencies**: T001
**Acceptance**:
- 4 test projects: Domain.UnitTests, Application.UnitTests, Application.IntegrationTests, WebApi.ContractTests
- NuGet packages: xUnit, FluentAssertions, Moq, Testcontainers, Bogus
- Test projects reference appropriate source projects
```bash
dotnet new xunit -n Domain.UnitTests
dotnet new xunit -n Application.UnitTests
dotnet new xunit -n Application.IntegrationTests
dotnet new xunit -n WebApi.ContractTests
```

---

### T147 [P] Create Order entity unit tests
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\tests\Domain.UnitTests\Entities\OrderTests.cs`
**Purpose**: Test Order business logic
**Dependencies**: T016, T146
**Acceptance**:
- Test state transitions: Complete(), Void(), Hold()
- Test business rules: TotalAmount calculation
- Test validation: Subtotal >= 0, VoidReason required when voided
- Test domain events: OrderCreatedEvent, OrderCompletedEvent
- All tests pass

---

### T148 [P] Create LoginCommandHandler unit tests
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\tests\Application.UnitTests\Features\Auth\LoginCommandHandlerTests.cs`
**Purpose**: Test login authentication logic
**Dependencies**: T042, T146
**Acceptance**:
- Test successful login with valid credentials
- Test failed login with invalid credentials
- Test account lockout after multiple failures
- Test token generation
- Mock DbContext and dependencies with Moq

---

### T149 [P] Create CreateOrderCommandHandler unit tests
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\tests\Application.UnitTests\Features\POS\CreateOrderCommandHandlerTests.cs`
**Purpose**: Test order creation logic
**Dependencies**: T047, T146
**Acceptance**:
- Test order creation with valid data
- Test OrderNumber generation
- Test inventory reservation
- Test validation failures
- Test transaction rollback on error

---

### T150 [P] Setup Testcontainers for integration tests
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\tests\Application.IntegrationTests\Infrastructure\TestcontainersSetup.cs`
**Purpose**: Configure PostgreSQL and Redis containers for integration tests
**Dependencies**: T146
**Acceptance**:
- PostgreSqlContainer configured and started before tests
- RedisContainer configured and started
- Database migrated with test schema
- Containers stopped after test run
- Fixtures for reusable container instances

---

### T151 [P] Create POS flow integration tests
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\tests\Application.IntegrationTests\Features\POSFlowTests.cs`
**Purpose**: Test complete POS transaction flow
**Dependencies**: T047-T052, T150
**Acceptance**:
- Test full flow: CreateOrder → AddPayment → CompleteOrder
- Test hold/resume order flow
- Test order return flow
- Verify database state after each step
- Verify inventory updates
- All tests use real database (Testcontainers)

---

### T152 [P] Create inventory management integration tests
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\tests\Application.IntegrationTests\Features\InventoryManagementTests.cs`
**Purpose**: Test inventory operations
**Dependencies**: T059-T065, T150
**Acceptance**:
- Test goods receipt flow: Create → Complete
- Test inventory issue flow: Create → Complete
- Test stocktake flow: Create → Finalize → Adjust inventory
- Test transfer between stores
- Verify inventory levels after operations

---

### T153 [P] Create customer loyalty integration tests
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\tests\Application.IntegrationTests\Features\CustomerLoyaltyTests.cs`
**Purpose**: Test loyalty points system
**Dependencies**: T052, T071-T072, T150
**Acceptance**:
- Test points earned on order completion
- Test points redemption
- Test points expiration
- Test customer group multiplier
- Verify loyalty transaction history

---

### T154 [P] Create AuthController contract tests
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\tests\WebApi.ContractTests\Controllers\AuthControllerTests.cs`
**Purpose**: Validate API contract for auth endpoints
**Dependencies**: T134, T146
**Acceptance**:
- Test endpoint exists and returns correct status codes
- Test request/response schema validation
- Test required fields validation
- Test authentication requirements
- Tests fail initially, pass after implementation

---

### T155 [P] Create POSController contract tests
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\tests\WebApi.ContractTests\Controllers\POSControllerTests.cs`
**Purpose**: Validate API contract for POS endpoints
**Dependencies**: T135, T146
**Acceptance**:
- Test all 9 endpoints
- Validate request/response schemas against OpenAPI spec
- Test authorization requirements
- Test error responses (400, 401, 404, etc.)

---

### T156 [P] Create InventoryController contract tests
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\tests\WebApi.ContractTests\Controllers\InventoryControllerTests.cs`
**Purpose**: Validate API contract for inventory endpoints
**Dependencies**: T136, T146
**Acceptance**:
- Test all inventory endpoints
- Validate pagination parameters
- Test filter combinations
- Test authorization by role

---

### T157 [P] Create CustomersController contract tests
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\tests\WebApi.ContractTests\Controllers\CustomersControllerTests.cs`
**Purpose**: Validate API contract for customer endpoints
**Dependencies**: T137, T146
**Acceptance**:
- Test customer CRUD endpoints
- Test search functionality
- Test loyalty points endpoints
- Test debt management endpoints

---

### T158 [P] Create ReportsController contract tests
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\tests\WebApi.ContractTests\Controllers\ReportsControllerTests.cs`
**Purpose**: Validate API contract for report endpoints
**Dependencies**: T139, T146
**Acceptance**:
- Test all 5 report endpoints
- Validate date range parameters
- Test response caching headers
- Verify authorization

---

### T159 [P] Create performance tests for critical operations
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\tests\Performance\CriticalOperationsTests.cs`
**Purpose**: Validate performance SLAs
**Dependencies**: T047-T052, T082-T086
**Acceptance**:
- Test payment transaction < 3 seconds (CompleteOrderCommand)
- Test sales report generation < 10 seconds (SalesReportQuery)
- Test product search < 500ms (SearchProductsQuery)
- Test dashboard load < 2 seconds (DashboardQuery)
- Tests run with realistic data volumes (10k orders, 1k products)
- Use k6 or JMeter for load testing

---

### T160 [P] Create SignalR hub tests
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\tests\WebApi.ContractTests\Hubs\NotificationHubTests.cs`
**Purpose**: Test SignalR notification hub
**Dependencies**: T142, T146
**Acceptance**:
- Test JoinStore/LeaveStore methods
- Test server-to-client message delivery
- Test authorization
- Use SignalR test client

---

### T161 [P] Create end-to-end test scenarios
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\tests\E2E\POSScenarios.cs`
**Purpose**: Test complete user workflows
**Dependencies**: T134-T145
**Acceptance**:
- Scenario 1: Cashier shift lifecycle (start → sell products → end shift)
- Scenario 2: Manager inventory management (receive goods → stocktake → adjust)
- Scenario 3: Customer journey (register → purchase → loyalty points → redeem)
- Scenario 4: Multi-store transfer (issue from Store A → receive at Store B)
- All scenarios use real API endpoints via HTTP client

---

### T162 Run all tests and verify coverage
**File**: N/A (CI/CD pipeline task)
**Purpose**: Execute full test suite and measure coverage
**Dependencies**: T147-T161
**Acceptance**:
- All unit tests pass (100+ tests)
- All integration tests pass (30+ tests)
- All contract tests pass (50+ tests)
- Code coverage >= 80% for Application layer
- Code coverage >= 70% for Domain layer
- Coverage report generated (Coverlet + ReportGenerator)
```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report
```

---

## Phase H: Polish & Deployment (T163-T170)

### T163 [P] Configure OpenAPI/Swagger documentation
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\src\WebApi\Program.cs`
**Purpose**: Generate comprehensive API documentation
**Dependencies**: T001, T134-T141
**Acceptance**:
- Swagger UI at /swagger
- OpenAPI 3.0 spec generated
- All endpoints documented with summaries and examples
- Request/response schemas included
- Authentication configured (JWT bearer token)
- XML comments enabled for detailed descriptions

---

### T164 [P] Create Dockerfile for API
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\Dockerfile`
**Purpose**: Containerize ASP.NET Core API
**Dependencies**: T001-T145
**Acceptance**:
- Multi-stage Dockerfile (build + runtime)
- Base image: mcr.microsoft.com/dotnet/aspnet:9.0
- Exposes port 8080
- Non-root user for security
- Optimized layer caching
- Image size < 300MB
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "WebApi.dll"]
```

---

### T165 [P] Update Docker Compose for production
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\docker-compose.prod.yml`
**Purpose**: Production-ready Docker Compose configuration
**Dependencies**: T008, T164
**Acceptance**:
- Services: API (3 replicas), PostgreSQL, Redis, Nginx (reverse proxy)
- Environment variables from .env file
- Health checks for all services
- Volume mounts for PostgreSQL data persistence
- Nginx SSL termination
- Restart policies: always

---

### T166 [P] Setup CI/CD pipeline (GitHub Actions)
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\.github\workflows\ci-cd.yml`
**Purpose**: Automate build, test, and deployment
**Dependencies**: T001-T165
**Acceptance**:
- Trigger on push to main and pull requests
- Steps: Restore → Build → Test → Publish → Docker Build → Deploy
- Run all tests (unit, integration, contract)
- Generate coverage report
- Build Docker image and push to registry
- Deploy to staging environment
- Notifications on failure (email/Slack)

---

### T167 [P] Create database backup and restore scripts
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\scripts\backup-db.sh`
**Purpose**: Automate database backups
**Dependencies**: T005, T122
**Acceptance**:
- Shell script using pg_dump
- Daily automated backups via cron
- Retention: 30 daily, 12 monthly, 7 yearly
- Backup to S3 or local storage
- Restore script: restore-db.sh
- Test backup restoration monthly

---

### T168 [P] Perform security audit
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\docs\security-audit.md`
**Purpose**: Validate security measures
**Dependencies**: T001-T165
**Acceptance**:
- OWASP Top 10 checklist completed
- SQL injection: Protected via parameterized queries ✓
- XSS: Protected via input validation ✓
- CSRF: Not applicable (stateless API) ✓
- Authentication: JWT with secure secret ✓
- Authorization: Role-based access control ✓
- Sensitive data: Passwords hashed with BCrypt ✓
- HTTPS: Required in production ✓
- Rate limiting: Implemented ✓
- Dependency vulnerabilities: Run `dotnet list package --vulnerable`
- Fix all high/critical vulnerabilities

---

### T169 [P] Performance optimization pass
**File**: Multiple files (queries, caching, indexing)
**Purpose**: Ensure performance SLAs are met
**Dependencies**: T001-T165
**Acceptance**:
- Profile slow queries with PostgreSQL explain analyze
- Add missing indexes identified by profiling
- Implement Redis caching for frequently accessed data (products, customer groups)
- Enable query result caching for reports (5-minute TTL)
- Use AsNoTracking() in all read-only queries
- Optimize AutoMapper mappings (use ProjectTo)
- Enable response compression in WebApi
- Target: p95 latency < 200ms for CRUD operations, < 3s for payment, < 10s for reports

---

### T170 Create quickstart guide and documentation
**File**: `E:\Work\Self\point-of-sale\backend\net-9-point-of-sales\README.md`
**Purpose**: Provide comprehensive onboarding documentation
**Dependencies**: T001-T169
**Acceptance**:
- README with project overview, tech stack, architecture diagram
- Prerequisites: .NET 9 SDK, Docker, PostgreSQL client
- Setup instructions: Clone → docker-compose up → migrate → seed → run
- Sample API calls with curl/Postman collection
- Environment variables documentation
- Deployment guide for production
- Troubleshooting section
- Link to OpenAPI spec (/swagger)
- Architecture decision records (ADRs) for key decisions

---

## Dependency Graph Summary

**Critical Path** (longest sequential chain):
```
T001 → T002 → T016 → T047 → T052 → T135 → T151 → T169 → T170
```

**Parallelizable Phases**:
- Phase B (T016-T041): All entity creation [26 parallel tasks]
- Phase C (T042-T095): All commands/queries grouped by module [54 tasks, ~7 parallel groups]
- Phase D (T096-T121): All EF configurations [26 parallel tasks]
- Phase E (T126-T133): All external services [8 parallel tasks]
- Phase F (T134-T145): All controllers and middleware [12 parallel tasks]
- Phase G (T147-T161): All test files [15 parallel tasks]

**High-Risk Dependencies** (must complete first):
1. T005 (DbContext) - Blocks all persistence work
2. T013 (MediatR behaviors) - Blocks all command/query handlers
3. T122 (Initial migration) - Blocks integration tests
4. T150 (Testcontainers) - Blocks all integration tests

---

## Parallel Execution Examples

**Week 1-2**: Foundation + Domain Entities (T001-T041)
- Developer A: T001-T015 (Foundation setup)
- Developer B: T016-T020, T036-T041 (Sales + Employee entities)
- Developer C: T021-T031 (Inventory entities)
- Developer D: T032-T035 (Customer entities)

**Week 3-4**: Application Layer Commands/Queries (T042-T095)
- Developer A: T042-T055 (Auth + POS module)
- Developer B: T056-T066 (Inventory module)
- Developer C: T067-T081 (Customers + Employees modules)
- Developer D: T082-T095 (Reports + Integrations modules)

**Week 5**: Infrastructure Persistence (T096-T125)
- Developer A: T096-T108 (Sales + Inventory configurations)
- Developer B: T109-T121 (Customer + Employee + Store configurations)
- Developer C: T122-T125 (Migrations, seed, JWT, password services)

**Week 6**: Infrastructure Services (T126-T133) + Presentation (T134-T145)
- Developer A: T126-T133 (External integrations, jobs, notifications)
- Developer B: T134-T141 (All API controllers)
- Developer C: T142-T145 (SignalR hubs + middleware)

**Week 7-8**: Testing (T146-T162)
- Developer A: T147-T149, T154-T158 (Unit tests + Contract tests)
- Developer B: T150-T153 (Integration tests)
- Developer C: T159-T162 (Performance tests + E2E scenarios)

**Week 9**: Polish & Deployment (T163-T170)
- All developers collaborate on final tasks

---

## Acceptance Criteria Checklist

**Per Task**:
- [ ] Code compiles without errors
- [ ] Code follows C# coding standards (StyleCop/EditorConfig)
- [ ] All dependencies installed and referenced
- [ ] Unit tests written and passing (where applicable)
- [ ] XML documentation comments added
- [ ] Code reviewed by peer
- [ ] Committed to feature branch

**Per Phase**:
- [ ] All phase tasks completed
- [ ] Integration tests passing
- [ ] No merge conflicts with main branch
- [ ] Performance benchmarks met
- [ ] Security review completed

**Final Acceptance** (T170 complete):
- [ ] All 170 tasks completed
- [ ] Full test suite passing (200+ tests)
- [ ] Code coverage >= 80%
- [ ] Performance SLAs validated (< 3s payment, < 10s reports)
- [ ] Security audit passed
- [ ] Docker deployment tested
- [ ] Documentation complete
- [ ] Demo to stakeholders successful
- [ ] Production deployment successful

---

## Notes for LLM Execution

**TDD Approach**:
1. For each command/query (Phase C), write the test first (in Phase G)
2. Run test, verify it fails
3. Implement handler
4. Run test again, verify it passes

**Incremental Testing**:
- After each entity (Phase B), write a basic unit test
- After each handler (Phase C), write unit test and update integration test
- After each controller (Phase F), write contract test

**Verification Points**:
- After T015: Verify solution builds
- After T041: Verify all entities created
- After T095: Verify all commands/queries registered in DI
- After T122: Verify migration applies successfully
- After T145: Verify API responds to /health endpoint
- After T162: Verify all tests pass

**Common Pitfalls to Avoid**:
- Don't skip T013 (pipeline behaviors) - critical for cross-cutting concerns
- Don't skip T122 (migration) before starting integration tests
- Don't skip T150 (Testcontainers) - required for realistic integration tests
- Don't implement controllers (Phase F) before handlers (Phase C) are complete
- Don't skip seed data (T123) - required for testing

---

*This tasks.md file provides a complete, executable roadmap for implementing the POS backend API. Each task is atomic, testable, and clearly defines acceptance criteria. The file is optimized for parallel execution while respecting dependencies.*
