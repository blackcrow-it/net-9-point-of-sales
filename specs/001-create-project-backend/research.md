# Technical Research: POS Backend API System

## Overview
This document consolidates technical research for building a Point-of-Sale (POS) backend API system using Clean Architecture with ASP.NET Core 9, targeting 6-20 retail stores with 50-100 employees.

**Technology Stack:**
- ASP.NET Core 9
- Clean Architecture
- MediatR (CQRS)
- PostgreSQL
- Redis
- SignalR
- Entity Framework Core 9

---

## 1. Clean Architecture in .NET 9

### Decision
Implement Clean Architecture with four distinct layers: Domain, Application, Infrastructure, and Presentation (API). Dependencies flow inward toward the Domain core, which has zero external dependencies.

### Rationale
- **Testability**: Inner layers can be tested independently without external dependencies
- **Maintainability**: Business logic remains isolated from infrastructure concerns
- **Flexibility**: Infrastructure implementations can be swapped without affecting business rules
- **Scalability**: Clear separation enables teams to work on different layers simultaneously
- **.NET 9 Support**: Enhanced with improved dependency injection and performance optimizations

### Layer Structure

#### Domain Layer (Core)
- Entities and Aggregates
- Value Objects
- Domain Events
- Domain Exceptions
- No external dependencies

#### Application Layer
- Use Cases (Commands & Queries via MediatR)
- DTOs and Mapping
- Interfaces for Infrastructure
- Validation (FluentValidation)
- Application Services

#### Infrastructure Layer
- Database Context (EF Core)
- External API Integrations (VNPAY, GHN, GHTK)
- Caching (Redis)
- File Storage
- Email/SMS Services
- Identity & Authentication

#### Presentation Layer (API)
- Controllers/Endpoints
- Middleware
- Filters
- API Documentation (OpenAPI)
- SignalR Hubs

### Cross-Cutting Concerns Management

**Decorator Pattern:**
- Ideal for logging, caching, authorization, and validation
- Wraps service behaviors without modifying core logic
- Example: Transaction decorators for command handlers

**MediatR Pipeline Behaviors:**
- Logging behavior for all requests
- Validation behavior with FluentValidation
- Performance monitoring behavior
- Transaction behavior for commands
- Caching behavior for queries

**Dependency Injection:**
- Services registered in Infrastructure layer
- Injected into Application layer via interfaces
- .NET 9's improved DI container for better performance

### Implementation Notes
```csharp
// Cross-cutting concerns via MediatR Pipeline
public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling {RequestName}", typeof(TRequest).Name);
        var response = await next();
        _logger.LogInformation("Handled {RequestName}", typeof(TRequest).Name);
        return response;
    }
}
```

### Alternatives Considered
- **Vertical Slice Architecture**: Rejected due to complexity in managing shared domain logic across 20 stores
- **Traditional N-Tier**: Rejected for lack of testability and tight coupling
- **Hexagonal Architecture**: Similar benefits but less familiar to .NET developers

---

## 2. CQRS with MediatR

### Decision
Implement CQRS pattern using MediatR library for command/query separation with pipeline behaviors for cross-cutting concerns.

### Rationale
- **Separation of Concerns**: Commands (write) and Queries (read) have different optimization needs
- **Scalability**: Read and write operations can be scaled independently
- **Performance**: Queries can bypass change tracking, use optimized projections
- **Clear Intent**: Explicit commands and queries improve code readability
- **Pipeline Behaviors**: Centralized cross-cutting concerns (validation, logging, caching)

### Important Distinction
**CQRS ≠ MediatR**: CQRS is an architectural pattern for separating read/write concerns. MediatR is a mediator pattern implementation that decouples components. They work well together but serve different purposes.

### Command Pattern
```csharp
// Command: Represents write operations
public record CreateOrderCommand : IRequest<OrderDto>
{
    public Guid StoreId { get; init; }
    public List<OrderItemDto> Items { get; init; }
    public decimal TotalAmount { get; init; }
}

// Command Handler
public class CreateOrderCommandHandler
    : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public async Task<OrderDto> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = Order.Create(
            request.StoreId,
            request.Items,
            request.TotalAmount);

        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<OrderDto>(order);
    }
}
```

### Query Pattern
```csharp
// Query: Represents read operations
public record GetOrdersByStoreQuery : IRequest<List<OrderDto>>
{
    public Guid StoreId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

// Query Handler with AsNoTracking for performance
public class GetOrdersByStoreQueryHandler
    : IRequestHandler<GetOrdersByStoreQuery, List<OrderDto>>
{
    private readonly IApplicationDbContext _context;

    public async Task<List<OrderDto>> Handle(
        GetOrdersByStoreQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.StoreId == request.StoreId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
```

### Pipeline Behaviors

**Validation Behavior:**
```csharp
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
            throw new ValidationException(failures);

        return await next();
    }
}
```

**Transaction Behavior (Commands only):**
```csharp
public class TransactionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IApplicationDbContext _context;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!IsCommand()) return await next();

        await using var transaction = await _context.Database
            .BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next();
            await transaction.CommitAsync(cancellationToken);
            return response;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private bool IsCommand() =>
        typeof(TRequest).Name.EndsWith("Command");
}
```

### Domain Events
```csharp
// Domain Event
public record OrderCreatedEvent : INotification
{
    public Guid OrderId { get; init; }
    public Guid StoreId { get; init; }
    public decimal TotalAmount { get; init; }
}

// Event Handler (can have multiple)
public class OrderCreatedEventHandler
    : INotificationHandler<OrderCreatedEvent>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public async Task Handle(
        OrderCreatedEvent notification,
        CancellationToken cancellationToken)
    {
        // Send SignalR notification
        await _hubContext.Clients
            .Group($"Store_{notification.StoreId}")
            .SendAsync("OrderCreated", notification, cancellationToken);
    }
}
```

### Implementation Notes
- Use synchronous domain events within transaction boundaries
- Use asynchronous events (RabbitMQ/Azure Service Bus) for cross-aggregate operations
- Pipeline behaviors execute in registration order
- Commands should always validate input using FluentValidation
- Queries should use AsNoTracking() for read-only operations

### Alternatives Considered
- **Manual separation without MediatR**: More boilerplate, harder to maintain
- **Event Sourcing with CQRS**: Overkill for POS system, adds unnecessary complexity
- **Separate read/write databases**: Not needed for 6-20 stores scale

---

## 3. PostgreSQL Schema Design

### Decision
Implement **hybrid multi-tenancy** using shared database with tenant isolation via `store_id` column on all tables, combined with **range partitioning** for time-series data and **row-level security (RLS)** for data isolation.

### Rationale
- **Cost-Effective**: Single database reduces operational overhead for 6-20 stores
- **Performance**: Partitioning improves query performance for 7-year retention
- **Scalability**: Can migrate to schema-per-tenant if store count exceeds 100
- **Compliance**: 7-year data retention requirement met via partitioning strategy
- **Query Optimization**: Partitions enable partition pruning for time-range queries

### Multi-Tenancy Strategy: Pool Model with Enhancements

**Core Approach:**
- All tenant data in shared tables with `store_id` as discriminator
- Every table includes `store_id` for filtering and future sharding
- Composite indexes on (`store_id`, `created_at`) for common queries

**Why Pool Model:**
- Simplified management for 6-20 stores
- Shared infrastructure reduces costs
- Easier cross-store reporting and analytics
- Natural fit for retail POS operations

### Table Partitioning for 7-Year Retention

**Range Partitioning Strategy:**
```sql
-- Parent table for orders
CREATE TABLE orders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    store_id UUID NOT NULL REFERENCES stores(id),
    order_number VARCHAR(50) NOT NULL,
    customer_id UUID REFERENCES customers(id),
    total_amount DECIMAL(18,2) NOT NULL,
    status VARCHAR(20) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
) PARTITION BY RANGE (created_at);

-- Create yearly partitions for 7-year retention
CREATE TABLE orders_2025 PARTITION OF orders
    FOR VALUES FROM ('2025-01-01') TO ('2026-01-01');

CREATE TABLE orders_2026 PARTITION OF orders
    FOR VALUES FROM ('2026-01-01') TO ('2027-01-01');

-- Continue for 7 years...

-- Index on each partition
CREATE INDEX idx_orders_2025_store_created
    ON orders_2025 (store_id, created_at DESC);
```

**Partitioning Benefits:**
- **Query Performance**: Partition pruning limits scans to relevant partitions
- **Archival**: Detach old partitions to cheaper storage without locking
- **Maintenance**: Vacuum and analyze operations are faster on smaller partitions
- **Deletion**: Drop entire partitions for expired data (faster than DELETE)

**Retention Management with pg_partman:**
```sql
-- Setup automatic partition management
SELECT partman.create_parent(
    p_parent_table := 'public.orders',
    p_control := 'created_at',
    p_type := 'native',
    p_interval := '1 year',
    p_premake := 3  -- Create 3 partitions in advance
);

-- Configure retention (7 years)
UPDATE partman.part_config
SET retention = '7 years',
    retention_keep_table = true,  -- Keep as separate table
    retention_keep_index = false
WHERE parent_table = 'public.orders';
```

### Indexing Strategies

**Multi-Tenant Indexes:**
```sql
-- Always include store_id first for tenant isolation
CREATE INDEX idx_orders_store_status
    ON orders (store_id, status, created_at DESC);

-- Covering index for common queries
CREATE INDEX idx_orders_store_customer
    ON orders (store_id, customer_id)
    INCLUDE (total_amount, status);

-- Partial index for active orders
CREATE INDEX idx_orders_active
    ON orders (store_id, created_at DESC)
    WHERE status IN ('pending', 'processing');
```

**Performance Considerations:**
- Use BRIN indexes for time-series columns with natural ordering
- Composite indexes with `store_id` first enable partition pruning
- Partial indexes for frequently queried subsets (e.g., active orders)
- Avoid over-indexing; each index adds write overhead

### Row-Level Security (RLS)

**Additional Isolation Layer:**
```sql
-- Enable RLS on orders table
ALTER TABLE orders ENABLE ROW LEVEL SECURITY;

-- Policy for store isolation
CREATE POLICY store_isolation_policy ON orders
    USING (store_id = current_setting('app.current_store_id')::UUID);

-- Set store context in application
SET app.current_store_id = 'uuid-of-store';
```

**Benefits:**
- Database-level enforcement of tenant isolation
- Protection against application bugs leaking data
- Defense-in-depth security strategy

### Archival Strategy for 7-Year Retention

**Hot/Warm/Cold Data Tiers:**

1. **Hot Data (0-1 year)**: Main tablespace, SSD storage, full indexes
2. **Warm Data (1-3 years)**: Separate tablespace, may use slower storage
3. **Cold Data (3-7 years)**: Detached partitions, compressed, minimal indexes

**Implementation:**
```sql
-- Move warm partition to different tablespace
ALTER TABLE orders_2023 SET TABLESPACE warm_data;

-- Detach cold partition (keeps data, removes from parent)
ALTER TABLE orders DETACH PARTITION orders_2019 CONCURRENTLY;

-- Compress detached partition
ALTER TABLE orders_2019 SET (
    autovacuum_enabled = false,
    fillfactor = 100
);
VACUUM FULL orders_2019;

-- Optional: Move to archive tablespace
ALTER TABLE orders_2019 SET TABLESPACE archive_data;
```

### Schema Design Patterns

**Denormalization for Performance:**
```sql
-- Store denormalized data to avoid joins
CREATE TABLE order_items (
    id UUID PRIMARY KEY,
    order_id UUID NOT NULL,
    store_id UUID NOT NULL,  -- Denormalized from orders
    product_id UUID NOT NULL,
    product_name VARCHAR(255),  -- Snapshot at order time
    unit_price DECIMAL(18,2),    -- Snapshot at order time
    quantity INTEGER NOT NULL,
    created_at TIMESTAMP NOT NULL
) PARTITION BY RANGE (created_at);
```

**Audit Trail Pattern:**
```sql
-- Separate audit table with longer retention
CREATE TABLE order_audit (
    id BIGSERIAL,
    order_id UUID NOT NULL,
    store_id UUID NOT NULL,
    action VARCHAR(20) NOT NULL,
    changed_by UUID NOT NULL,
    changed_at TIMESTAMP NOT NULL DEFAULT NOW(),
    old_values JSONB,
    new_values JSONB
) PARTITION BY RANGE (changed_at);
```

### Implementation Notes
- Use `gen_random_uuid()` for better distribution across partitions
- Partition maintenance should run during off-peak hours
- Monitor partition sizes; rebalance if needed
- Test restore procedures for archived partitions
- Plan for future sharding if store count exceeds 100

### Alternatives Considered
- **Schema-per-tenant**: Too complex for 6-20 stores, creates 100+ schemas at max scale
- **Database-per-tenant**: Operational nightmare, expensive infrastructure
- **Citus distributed PostgreSQL**: Overkill for this scale, adds complexity
- **Monthly partitions**: More partitions to manage, yearly is optimal for 7-year retention

---

## 4. Redis Patterns

### Decision
Implement Redis for four primary use cases: distributed caching (cache-aside pattern), session storage, job queuing, and SignalR backplane for real-time notifications.

### Rationale
- **Performance**: Sub-millisecond latency for cached data
- **Scalability**: Distributed cache shared across API instances
- **Reliability**: Replication ensures high availability
- **Flexibility**: Supports multiple access patterns (cache, pub/sub, queuing)
- **Cost-Effective**: Single Redis instance serves multiple purposes

### 1. Cache-Aside Pattern (Distributed Caching)

**Implementation:**
```csharp
public class ProductService
{
    private readonly IDistributedCache _cache;
    private readonly IProductRepository _repository;

    public async Task<Product> GetProductAsync(Guid id)
    {
        var cacheKey = $"product:{id}";

        // Try to get from cache
        var cachedProduct = await _cache.GetStringAsync(cacheKey);
        if (cachedProduct != null)
        {
            return JsonSerializer.Deserialize<Product>(cachedProduct);
        }

        // Cache miss - get from database
        var product = await _repository.GetByIdAsync(id);
        if (product == null) return null;

        // Store in cache with expiration
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(product),
            options);

        return product;
    }
}
```

**Caching Strategy:**
- **Product Catalog**: 30-60 minute TTL (frequent reads, infrequent updates)
- **Store Configuration**: 1-hour TTL (rarely changes)
- **User Sessions**: 30-minute sliding expiration
- **Reports/Analytics**: 5-15 minute TTL (acceptable staleness)

**Cache Invalidation:**
```csharp
public class UpdateProductCommandHandler
    : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public async Task<ProductDto> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FindAsync(request.Id, cancellationToken);

        // Update product
        product.Update(request.Name, request.Price);
        await _context.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cache.RemoveAsync($"product:{request.Id}");

        return _mapper.Map<ProductDto>(product);
    }
}
```

### 2. Session Storage

**ASP.NET Core Integration:**
```csharp
// Program.cs
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration
        .GetConnectionString("Redis");
    options.InstanceName = "POS_";
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
```

**Session Data Pattern:**
```csharp
public class CartService
{
    private readonly IHttpContextAccessor _httpContext;

    public async Task AddToCartAsync(Guid productId, int quantity)
    {
        var session = _httpContext.HttpContext.Session;
        var cartKey = "shopping_cart";

        var cartJson = await session.GetStringAsync(cartKey);
        var cart = cartJson != null
            ? JsonSerializer.Deserialize<Cart>(cartJson)
            : new Cart();

        cart.AddItem(productId, quantity);

        await session.SetStringAsync(
            cartKey,
            JsonSerializer.Serialize(cart));
    }
}
```

**Use Cases for Session Storage:**
- Shopping cart before checkout
- User preferences and UI state
- Multi-step form data
- Draft orders

### 3. Job Queue Pattern

**Using Hangfire with Redis:**
```csharp
// Program.cs
builder.Services.AddHangfire(config =>
{
    config.UseRedisStorage(
        builder.Configuration.GetConnectionString("Redis"),
        new RedisStorageOptions
        {
            Prefix = "POS:Hangfire:"
        });
});

builder.Services.AddHangfireServer();

// Background job examples
public class OrderService
{
    private readonly IBackgroundJobClient _jobClient;

    public async Task<Order> CreateOrderAsync(CreateOrderCommand command)
    {
        var order = await _orderRepository.CreateAsync(command);

        // Queue background job for receipt email
        _jobClient.Enqueue<IEmailService>(
            x => x.SendOrderConfirmationAsync(order.Id));

        // Schedule report generation after 24 hours
        _jobClient.Schedule<IReportService>(
            x => x.GenerateDailySalesReportAsync(order.StoreId),
            TimeSpan.FromHours(24));

        return order;
    }
}
```

**Job Queuing Use Cases:**
- Email/SMS notifications
- Report generation
- Data synchronization between stores
- Payment processing callbacks
- Inventory updates

### 4. Pub/Sub for Real-Time Events

**Redis Pub/Sub Pattern:**
```csharp
public class OrderEventPublisher
{
    private readonly IConnectionMultiplexer _redis;

    public async Task PublishOrderCreatedAsync(OrderCreatedEvent orderEvent)
    {
        var subscriber = _redis.GetSubscriber();
        var channel = $"orders:{orderEvent.StoreId}";
        var message = JsonSerializer.Serialize(orderEvent);

        await subscriber.PublishAsync(
            RedisChannel.Literal(channel),
            message);
    }
}

public class OrderEventSubscriber : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = _redis.GetSubscriber();

        await subscriber.SubscribeAsync(
            RedisChannel.Pattern("orders:*"),
            async (channel, message) =>
            {
                var orderEvent = JsonSerializer
                    .Deserialize<OrderCreatedEvent>(message);

                // Process event
                await ProcessOrderEventAsync(orderEvent);
            });
    }
}
```

### Redis Configuration Best Practices

**Connection Management:**
```csharp
// Singleton ConnectionMultiplexer
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(
        builder.Configuration.GetConnectionString("Redis"));

    configuration.AbortOnConnectFail = false;
    configuration.ConnectRetry = 3;
    configuration.ConnectTimeout = 5000;
    configuration.SyncTimeout = 5000;
    configuration.KeepAlive = 60;

    return ConnectionMultiplexer.Connect(configuration);
});
```

**Memory Management:**
```redis
# redis.conf
maxmemory 2gb
maxmemory-policy allkeys-lru  # Evict least recently used keys
save 900 1                     # Persistence for job queue
save 300 10
save 60 10000
```

### High Availability Setup

**Redis Sentinel for Failover:**
```csharp
var sentinelConfig = new ConfigurationOptions
{
    ServiceName = "pos-master",
    EndPoints =
    {
        { "sentinel1", 26379 },
        { "sentinel2", 26379 },
        { "sentinel3", 26379 }
    },
    TieBreaker = "",
    CommandMap = CommandMap.Sentinel
};

var connection = await ConnectionMultiplexer
    .ConnectAsync(sentinelConfig);
```

**Replication Benefits:**
- Automatic failover on master failure
- Read replicas for scaling read operations
- Data durability through persistence

### Implementation Notes
- Use Redis 7+ for improved performance and features
- Enable AOF (Append-Only File) for job queue persistence
- Monitor memory usage; plan for vertical scaling if approaching limits
- Use separate Redis instances for critical vs. non-critical data
- Implement circuit breaker pattern for Redis failures
- Use pipelining for batch operations to reduce round trips

### Alternatives Considered
- **Memcached**: Lacks persistence and advanced data structures needed for job queuing
- **In-Memory Cache**: Not shared across API instances, won't work in scaled environment
- **SQL Server as cache**: Too slow for sub-millisecond requirements
- **Azure Redis Cache**: Suitable, but Redis on VM offers more control and lower cost

---

## 5. SignalR Real-Time Features

### Decision
Implement SignalR with Redis backplane for real-time notifications across multiple server instances, using WebSockets with automatic fallback to Server-Sent Events and Long Polling.

### Rationale
- **Real-Time Updates**: Critical for POS operations (order status, inventory updates)
- **Scalability**: Redis backplane enables horizontal scaling across API instances
- **Reliability**: Automatic transport fallback ensures connection stability
- **Developer Experience**: Native .NET 9 integration with strongly-typed hubs
- **Cross-Platform**: Supports web, mobile, and desktop clients

### Architecture with Redis Backplane

**How It Works:**
1. Client connects to any API instance
2. Client connection info stored in Redis
3. Server publishes message to Redis pub/sub channel
4. Redis broadcasts to all API instances
5. Each instance delivers to its connected clients

```
┌─────────┐         ┌─────────────┐         ┌─────────────┐
│ Client  │────────▶│  API 1      │────────▶│             │
│   A     │         │  (SignalR)  │         │             │
└─────────┘         └─────────────┘         │             │
                            │                │    Redis    │
┌─────────┐                 │                │  Backplane  │
│ Client  │         ┌───────▼───────┐        │             │
│   B     │────────▶│  API 2        │────────▶│             │
└─────────┘         │  (SignalR)    │        │             │
                    └───────────────┘        └─────────────┘
```

**Configuration:**
```csharp
// Program.cs
builder.Services.AddSignalR()
    .AddStackExchangeRedis(
        builder.Configuration.GetConnectionString("Redis"),
        options =>
        {
            options.Configuration.ChannelPrefix =
                RedisChannel.Literal("POS:SignalR:");
        });
```

### Notification Hub Implementation

**Strongly-Typed Hub:**
```csharp
// Hub interface for type safety
public interface INotificationClient
{
    Task OrderCreated(OrderNotification notification);
    Task OrderStatusChanged(OrderStatusNotification notification);
    Task InventoryUpdated(InventoryNotification notification);
    Task PaymentReceived(PaymentNotification notification);
}

// SignalR Hub
public class NotificationHub : Hub<INotificationClient>
{
    private readonly ILogger<NotificationHub> _logger;

    public override async Task OnConnectedAsync()
    {
        var storeId = Context.GetHttpContext()
            .Request.Query["storeId"].ToString();

        if (!string.IsNullOrEmpty(storeId))
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"Store_{storeId}");

            _logger.LogInformation(
                "Client {ConnectionId} joined store group {StoreId}",
                Context.ConnectionId,
                storeId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        _logger.LogInformation(
            "Client {ConnectionId} disconnected",
            Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }
}
```

### Notification Patterns

**1. Broadcast to Store Group:**
```csharp
public class OrderNotificationService
{
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;

    public async Task NotifyOrderCreatedAsync(Order order)
    {
        var notification = new OrderNotification
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt
        };

        // Send to all clients in store group
        await _hubContext.Clients
            .Group($"Store_{order.StoreId}")
            .OrderCreated(notification);
    }
}
```

**2. Targeted User Notification:**
```csharp
public async Task NotifyUserAsync(Guid userId, string message)
{
    var connectionId = await GetUserConnectionIdAsync(userId);

    if (connectionId != null)
    {
        await _hubContext.Clients
            .Client(connectionId)
            .ReceiveMessage(message);
    }
}
```

**3. Broadcast to All Clients:**
```csharp
public async Task BroadcastSystemMessageAsync(string message)
{
    await _hubContext.Clients.All.ReceiveSystemMessage(message);
}
```

### Scalability Considerations

**Sticky Sessions:**
- **Not Required**: Redis backplane eliminates need for sticky sessions
- **Optional**: Can improve performance by reducing Redis traffic
- **Load Balancer Config**: Configure if using sticky sessions

```nginx
# Nginx upstream configuration
upstream signalr_backend {
    ip_hash;  # Sticky sessions based on IP
    server api1.example.com:5000;
    server api2.example.com:5000;
}
```

**Message Throughput:**
- Redis backplane reduces max throughput vs. direct connections
- Every message sent to N nodes in cluster
- Trade-off between scalability and throughput

**Performance Metrics:**
- Single server: ~100K messages/sec
- With Redis backplane: ~10K messages/sec (acceptable for POS)
- WebSocket connections: Up to 10K per instance

**When Redis Backplane Works Well:**
- Server-controlled broadcast scenarios (POS notifications)
- Low to medium message frequency
- Need for horizontal scaling

**When to Avoid:**
- High-frequency real-time (e.g., multiplayer games)
- Client-to-client communication heavy applications

### Client Integration

**JavaScript Client:**
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl(`/hubs/notifications?storeId=${storeId}`, {
        accessTokenFactory: () => getAccessToken()
    })
    .withAutomaticReconnect([0, 2000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.on("OrderCreated", (notification) => {
    console.log("New order:", notification);
    updateOrderList(notification);
});

await connection.start();
```

**.NET Client:**
```csharp
var connection = new HubConnectionBuilder()
    .WithUrl("https://api.example.com/hubs/notifications",
        options => options.AccessTokenProvider =
            async () => await GetAccessTokenAsync())
    .WithAutomaticReconnect()
    .Build();

connection.On<OrderNotification>("OrderCreated", notification =>
{
    Console.WriteLine($"Order {notification.OrderNumber} created");
});

await connection.StartAsync();
```

### Production Deployment Considerations

**Redis in Same Data Center:**
- Critical for performance
- Network latency degrades SignalR experience
- Co-locate Redis with API instances

**Connection Limits:**
```json
{
  "Azure:SignalR": {
    "ConnectionString": "...",
    "ConnectionCount": 5  // Connections per instance to Redis
  }
}
```

**Monitoring:**
```csharp
// Track connection metrics
public class SignalRMetrics : BackgroundService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var activeConnections = GetActiveConnectionCount();
            _logger.LogInformation(
                "Active SignalR connections: {Count}",
                activeConnections);

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
```

### Message Persistence Handling

**Important Limitation:**
- SignalR does not buffer messages
- Messages sent while client disconnected are lost
- Server restart loses in-flight messages

**Solution Patterns:**
```csharp
// Store critical notifications for retrieval
public class NotificationStore
{
    private readonly IDistributedCache _cache;

    public async Task StoreNotificationAsync(
        Guid userId,
        Notification notification)
    {
        var key = $"notifications:{userId}";
        var notifications = await GetUserNotificationsAsync(userId);
        notifications.Add(notification);

        await _cache.SetStringAsync(
            key,
            JsonSerializer.Serialize(notifications),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            });
    }

    public async Task<List<Notification>> GetMissedNotificationsAsync(
        Guid userId,
        DateTime since)
    {
        var notifications = await GetUserNotificationsAsync(userId);
        return notifications.Where(n => n.CreatedAt > since).ToList();
    }
}
```

### Implementation Notes
- Use WebSocket transport when possible for lowest latency
- Implement reconnection logic with exponential backoff
- Store critical notifications in database/cache for retrieval
- Monitor Redis memory usage; SignalR backplane stores connection data
- Plan for Redis failure: implement graceful degradation
- Use separate Redis instance for SignalR vs. caching if high traffic

### Alternatives Considered
- **Azure SignalR Service**: More expensive, but eliminates backplane management
- **Polling from Client**: Higher latency, more server load
- **Server-Sent Events (SSE)**: Uni-directional only, less browser support
- **WebSockets without SignalR**: More code, no automatic fallback

---

## 6. External Integrations

### Decision
Integrate VNPAY for payments and GHN/GHTK for shipping using Polly library for resilient HTTP calls with retry policies, circuit breakers, and timeout handling.

### Rationale
- **Resilience**: External APIs can fail; need automatic retry and fallback
- **User Experience**: Transient failures shouldn't break checkout flow
- **Reliability**: Circuit breaker prevents cascading failures
- **Observability**: Centralized error handling and logging
- **Compliance**: Payment and shipping are critical for POS operations

### VNPAY Payment Integration

**Security Best Practices:**
```csharp
public class VNPaySettings
{
    public string TmnCode { get; set; }      // Merchant code
    public string HashSecret { get; set; }   // Secret key
    public string Url { get; set; }          // Payment URL
    public string ReturnUrl { get; set; }    // Callback URL
}

// Secure configuration
// appsettings.json (use Azure Key Vault in production)
{
  "VNPay": {
    "TmnCode": "YOUR_TMN_CODE",
    "HashSecret": "YOUR_SECRET_KEY",
    "Url": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "ReturnUrl": "https://yourapp.com/payment/callback"
  }
}
```

**Payment Request:**
```csharp
public class VNPayService
{
    private readonly VNPaySettings _settings;
    private readonly HttpClient _httpClient;

    public async Task<string> CreatePaymentUrlAsync(
        Order order,
        string ipAddress)
    {
        // Validate and sanitize IP address
        if (!IPAddress.TryParse(ipAddress, out _))
            throw new ArgumentException("Invalid IP address");

        var vnpayData = new VNPayRequestData
        {
            vnp_Version = "2.1.0",
            vnp_Command = "pay",
            vnp_TmnCode = _settings.TmnCode,
            vnp_Amount = (order.TotalAmount * 100).ToString("F0"), // VND
            vnp_CreateDate = DateTime.Now.ToString("yyyyMMddHHmmss"),
            vnp_CurrCode = "VND",
            vnp_IpAddr = ipAddress,
            vnp_Locale = "vn",
            vnp_OrderInfo = $"Thanh toan don hang {order.OrderNumber}",
            vnp_OrderType = "other",
            vnp_ReturnUrl = _settings.ReturnUrl,
            vnp_TxnRef = order.OrderNumber
        };

        // Generate secure hash
        var signData = BuildSignData(vnpayData);
        vnpayData.vnp_SecureHash = HmacSHA512(
            _settings.HashSecret,
            signData);

        return BuildPaymentUrl(vnpayData);
    }

    private string HmacSHA512(string key, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);

        using var hmac = new HMACSHA512(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);
        return BitConverter.ToString(hashBytes)
            .Replace("-", "").ToLower();
    }
}
```

**IPN (Instant Payment Notification) Handler:**
```csharp
[ApiController]
[Route("api/payment/vnpay")]
public class VNPayCallbackController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly VNPaySettings _settings;
    private readonly ILogger<VNPayCallbackController> _logger;

    // Track processed transactions (idempotency)
    private readonly IDistributedCache _cache;

    [HttpGet("callback")]
    public async Task<IActionResult> PaymentCallback(
        [FromQuery] VNPayResponse response)
    {
        // Validate secure hash
        if (!ValidateSignature(response))
        {
            _logger.LogWarning(
                "Invalid signature for transaction {TxnRef}",
                response.vnp_TxnRef);
            return BadRequest("Invalid signature");
        }

        // Idempotency check - VNPay may send multiple IPNs
        var cacheKey = $"vnpay:processed:{response.vnp_TxnRef}";
        var alreadyProcessed = await _cache.GetStringAsync(cacheKey);

        if (alreadyProcessed != null)
        {
            _logger.LogInformation(
                "Transaction {TxnRef} already processed",
                response.vnp_TxnRef);
            return Ok(new { RspCode = "00", Message = "Confirm Success" });
        }

        // Process payment
        var command = new ProcessPaymentCommand
        {
            OrderNumber = response.vnp_TxnRef,
            TransactionId = response.vnp_TransactionNo,
            Amount = decimal.Parse(response.vnp_Amount) / 100,
            Status = response.vnp_ResponseCode == "00"
                ? PaymentStatus.Success
                : PaymentStatus.Failed,
            PaymentMethod = "VNPAY"
        };

        await _mediator.Send(command);

        // Mark as processed
        await _cache.SetStringAsync(
            cacheKey,
            "processed",
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            });

        return Ok(new { RspCode = "00", Message = "Confirm Success" });
    }
}
```

### GHN/GHTK Shipping Integration

**Service Abstraction:**
```csharp
public interface IShippingService
{
    Task<ShippingRateResponse> CalculateRateAsync(
        ShippingRateRequest request);
    Task<CreateShipmentResponse> CreateShipmentAsync(
        CreateShipmentRequest request);
    Task<TrackingResponse> TrackShipmentAsync(string trackingNumber);
}

public class GHNShippingService : IShippingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GHNShippingService> _logger;

    public GHNShippingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://dev-online-gateway.ghn.vn");
        _httpClient.DefaultRequestHeaders.Add("Token", "YOUR_GHN_TOKEN");
    }

    public async Task<CreateShipmentResponse> CreateShipmentAsync(
        CreateShipmentRequest request)
    {
        var ghnRequest = new
        {
            payment_type_id = 2, // COD
            note = request.Note,
            required_note = "KHONGCHOXEMHANG",
            from_name = request.SenderName,
            from_phone = request.SenderPhone,
            from_address = request.SenderAddress,
            from_ward_name = request.SenderWard,
            from_district_name = request.SenderDistrict,
            from_province_name = request.SenderProvince,
            to_name = request.RecipientName,
            to_phone = request.RecipientPhone,
            to_address = request.RecipientAddress,
            to_ward_code = request.RecipientWardCode,
            to_district_id = request.RecipientDistrictId,
            weight = request.Weight,
            length = request.Length,
            width = request.Width,
            height = request.Height,
            service_id = 53320,
            service_type_id = 2,
            cod_amount = request.CODAmount,
            items = request.Items.Select(i => new
            {
                name = i.Name,
                quantity = i.Quantity,
                price = i.Price
            }).ToList()
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/shiip/public-api/v2/shipping-order/create",
            ghnRequest);

        response.EnsureSuccessStatusCode();

        var result = await response
            .Content
            .ReadFromJsonAsync<GHNCreateShipmentResponse>();

        return new CreateShipmentResponse
        {
            TrackingNumber = result.Data.order_code,
            ShippingFee = result.Data.total_fee,
            ExpectedDeliveryDate = result.Data.expected_delivery_time
        };
    }
}
```

### Polly Resilience Policies

**Retry Policy with Exponential Backoff:**
```csharp
public static class HttpClientExtensions
{
    public static IHttpClientBuilder AddResiliencePolicies(
        this IHttpClientBuilder builder)
    {
        // Retry policy
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    var logger = context.GetLogger();
                    logger.LogWarning(
                        "Retry {RetryCount} after {Delay}s due to {Reason}",
                        retryCount,
                        timespan.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                });

        // Circuit breaker policy
        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, duration) =>
                {
                    // Log circuit breaker opened
                },
                onReset: () =>
                {
                    // Log circuit breaker reset
                });

        // Timeout policy
        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromSeconds(10));

        // Combine policies
        builder.AddPolicyHandler(
            Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy));

        return builder;
    }
}
```

**Service Registration:**
```csharp
// Program.cs
builder.Services.AddHttpClient<IVNPayService, VNPayService>()
    .AddResiliencePolicies();

builder.Services.AddHttpClient<IGHNShippingService, GHNShippingService>()
    .AddResiliencePolicies();

builder.Services.AddHttpClient<IGHTKShippingService, GHTKShippingService>()
    .AddResiliencePolicies();
```

### Fallback Strategy

**Shipping Provider Fallback:**
```csharp
public class ShippingService
{
    private readonly IGHNShippingService _ghn;
    private readonly IGHTKShippingService _ghtk;
    private readonly ILogger<ShippingService> _logger;

    public async Task<CreateShipmentResponse> CreateShipmentAsync(
        CreateShipmentRequest request)
    {
        try
        {
            // Try primary provider (GHN)
            return await _ghn.CreateShipmentAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GHN shipment creation failed, trying GHTK");

            try
            {
                // Fallback to secondary provider (GHTK)
                return await _ghtk.CreateShipmentAsync(request);
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(
                    fallbackEx,
                    "Both shipping providers failed");
                throw new ShippingServiceException(
                    "All shipping providers unavailable",
                    fallbackEx);
            }
        }
    }
}
```

### Webhook Security

**Signature Verification:**
```csharp
public class WebhookValidator
{
    public bool ValidateGHNWebhook(
        string signature,
        string payload,
        string secret)
    {
        var computedSignature = ComputeHMAC(secret, payload);
        return signature.Equals(
            computedSignature,
            StringComparison.OrdinalIgnoreCase);
    }

    private string ComputeHMAC(string secret, string payload)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(payloadBytes);
        return Convert.ToBase64String(hashBytes);
    }
}
```

### Testing External APIs

**Sandbox Environment:**
```json
{
  "ExternalAPIs": {
    "VNPay": {
      "Environment": "Sandbox",
      "SandboxUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
      "ProductionUrl": "https://vnpayment.vn/paymentv2/vpcpay.html"
    },
    "GHN": {
      "Environment": "Development",
      "DevUrl": "https://dev-online-gateway.ghn.vn",
      "ProductionUrl": "https://online-gateway.ghn.vn"
    }
  }
}
```

**Mock Services for Testing:**
```csharp
public class MockVNPayService : IVNPayService
{
    public async Task<string> CreatePaymentUrlAsync(
        Order order,
        string ipAddress)
    {
        // Return mock URL for testing
        return $"https://mock-vnpay.test/pay?order={order.OrderNumber}";
    }
}
```

### Implementation Notes
- Never expose secrets in client-side code
- Always validate and sanitize IP addresses
- Implement idempotent webhook handlers (VNPay sends multiple IPNs)
- Monitor webhook endpoints for unusual patterns
- Use separate credentials for sandbox vs. production
- Log all external API calls for debugging
- Implement alerting for circuit breaker trips
- Keep external API SDKs updated

### Alternatives Considered
- **Direct HTTP calls without Polly**: No resilience, harder to maintain
- **Azure Logic Apps for integrations**: Added complexity, less control
- **Rebus/MassTransit for messaging**: Overkill for HTTP-based APIs
- **Manual retry loops**: Error-prone, Polly provides battle-tested patterns

---

## 7. Security

### Decision
Implement JWT-based authentication with refresh tokens, claims-based RBAC, BCrypt password hashing, and built-in .NET 9 rate limiting middleware.

### Rationale
- **Industry Standard**: JWT is widely adopted and well-supported
- **Stateless**: Tokens contain claims, reducing database lookups
- **Scalability**: No server-side session storage required
- **Security**: Refresh tokens minimize access token exposure
- **Compliance**: BCrypt meets security standards for password storage
- **Protection**: Rate limiting prevents brute force and DDoS attacks

### JWT Authentication with Refresh Tokens

**Token Configuration:**
```csharp
public class JwtSettings
{
    public string SecretKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}

// appsettings.json (use Azure Key Vault in production)
{
  "JwtSettings": {
    "SecretKey": "your-256-bit-secret-key-here-minimum-32-characters",
    "Issuer": "https://yourapi.com",
    "Audience": "https://yourapp.com",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

**JWT Service Implementation:**
```csharp
public class JwtTokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IApplicationDbContext _context;

    public async Task<AuthenticationResponse> GenerateTokensAsync(
        User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id);

        return new AuthenticationResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = _jwtSettings.AccessTokenExpirationMinutes * 60,
            TokenType = "Bearer"
        };
    }

    private string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("store_id", user.StoreId.ToString()),
            new(ClaimTypes.Name, user.FullName)
        };

        // Add role claims
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
        }

        // Add permission claims
        foreach (var permission in user.GetPermissions())
        {
            claims.Add(new Claim("permission", permission));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                _jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId)
    {
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(
                _jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        // Revoke previous refresh tokens (token rotation)
        var existingTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in existingTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }
}
```

**Refresh Token Endpoint:**
```csharp
[HttpPost("refresh")]
public async Task<IActionResult> RefreshToken(
    [FromBody] RefreshTokenRequest request)
{
    // Validate refresh token
    var refreshToken = await _context.RefreshTokens
        .Include(rt => rt.User)
        .FirstOrDefaultAsync(rt =>
            rt.Token == request.RefreshToken &&
            !rt.IsRevoked &&
            rt.ExpiresAt > DateTime.UtcNow);

    if (refreshToken == null)
        return Unauthorized(new { error = "Invalid refresh token" });

    // Generate new token pair
    var response = await _jwtTokenService
        .GenerateTokensAsync(refreshToken.User);

    return Ok(response);
}
```

**JWT Validation Configuration:**
```csharp
// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration
            .GetSection("JwtSettings")
            .Get<JwtSettings>();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero  // Eliminate clock skew
        };

        // SignalR token from query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });
```

### Role-Based Access Control (RBAC) with Claims

**Permission System:**
```csharp
public static class Permissions
{
    // Order permissions
    public const string ViewOrders = "orders.view";
    public const string CreateOrders = "orders.create";
    public const string UpdateOrders = "orders.update";
    public const string DeleteOrders = "orders.delete";

    // Product permissions
    public const string ManageProducts = "products.manage";

    // Report permissions
    public const string ViewReports = "reports.view";
    public const string ExportReports = "reports.export";

    // User management
    public const string ManageUsers = "users.manage";
}

// Role definition
public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<string> Permissions { get; set; }
}

// Predefined roles
public static class Roles
{
    public static Role Admin => new()
    {
        Name = "Admin",
        Permissions = new List<string>
        {
            Permissions.ViewOrders,
            Permissions.CreateOrders,
            Permissions.UpdateOrders,
            Permissions.DeleteOrders,
            Permissions.ManageProducts,
            Permissions.ViewReports,
            Permissions.ExportReports,
            Permissions.ManageUsers
        }
    };

    public static Role Cashier => new()
    {
        Name = "Cashier",
        Permissions = new List<string>
        {
            Permissions.ViewOrders,
            Permissions.CreateOrders,
            Permissions.UpdateOrders
        }
    };

    public static Role Manager => new()
    {
        Name = "Manager",
        Permissions = new List<string>
        {
            Permissions.ViewOrders,
            Permissions.CreateOrders,
            Permissions.UpdateOrders,
            Permissions.ManageProducts,
            Permissions.ViewReports,
            Permissions.ExportReports
        }
    };
}
```

**Claims-Based Authorization:**
```csharp
// Custom authorization attribute
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
    {
        Policy = permission;
    }
}

// Policy-based authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Permissions.CreateOrders, policy =>
        policy.RequireClaim("permission", Permissions.CreateOrders));

    options.AddPolicy(Permissions.ManageProducts, policy =>
        policy.RequireClaim("permission", Permissions.ManageProducts));

    options.AddPolicy(Permissions.ViewReports, policy =>
        policy.RequireClaim("permission", Permissions.ViewReports));
});

// Usage in controllers
[HttpPost]
[RequirePermission(Permissions.CreateOrders)]
public async Task<IActionResult> CreateOrder(
    [FromBody] CreateOrderCommand command)
{
    var result = await _mediator.Send(command);
    return Ok(result);
}

// Custom authorization handler for complex logic
public class StoreResourceAuthorizationHandler
    : AuthorizationHandler<SameStoreRequirement, Order>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SameStoreRequirement requirement,
        Order resource)
    {
        var userStoreId = context.User
            .FindFirst("store_id")?.Value;

        if (userStoreId != null &&
            resource.StoreId.ToString() == userStoreId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
```

### Password Hashing with BCrypt

**Implementation:**
```csharp
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;  // BCrypt cost factor

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty");

        return BCrypt.Net.BCrypt.HashPassword(
            password,
            workFactor: WorkFactor);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}

// User registration
public class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, UserDto>
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IApplicationDbContext _context;

    public async Task<UserDto> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        // Validate password strength
        if (!IsPasswordStrong(request.Password))
            throw new ValidationException(
                "Password must be at least 8 characters with uppercase, lowercase, number, and special character");

        var user = new User
        {
            Email = request.Email.ToLowerInvariant(),
            FullName = request.FullName,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            StoreId = request.StoreId,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }

    private bool IsPasswordStrong(string password)
    {
        return password.Length >= 8 &&
               password.Any(char.IsUpper) &&
               password.Any(char.IsLower) &&
               password.Any(char.IsDigit) &&
               password.Any(ch => !char.IsLetterOrDigit(ch));
    }
}
```

### Rate Limiting (.NET 9 Built-in Middleware)

**Configuration:**
```csharp
// Program.cs
builder.Services.AddRateLimiter(options =>
{
    // Fixed window limiter for general API
    options.AddFixedWindowLimiter("api", options =>
    {
        options.PermitLimit = 100;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 10;
    });

    // Sliding window for authentication endpoints
    options.AddSlidingWindowLimiter("auth", options =>
    {
        options.PermitLimit = 5;
        options.Window = TimeSpan.FromMinutes(1);
        options.SegmentsPerWindow = 4;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 0;
    });

    // Token bucket for payment processing
    options.AddTokenBucketLimiter("payment", options =>
    {
        options.TokenLimit = 10;
        options.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
        options.TokensPerPeriod = 5;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 5;
    });

    // Concurrency limiter for reports
    options.AddConcurrencyLimiter("reports", options =>
    {
        options.PermitLimit = 10;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 20;
    });

    // Custom rejection response
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = 429;

        if (context.Lease.TryGetMetadata(
            MetadataName.RetryAfter,
            out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                retryAfter.TotalSeconds.ToString();
        }

        await context.HttpContext.Response.WriteAsJsonAsync(
            new
            {
                error = "Too many requests",
                retryAfter = retryAfter?.TotalSeconds
            },
            cancellationToken);
    };
});

app.UseRateLimiter();
```

**Apply to Endpoints:**
```csharp
// Global rate limiting
app.MapControllers().RequireRateLimiting("api");

// Specific endpoint rate limiting
[HttpPost("login")]
[EnableRateLimiting("auth")]
public async Task<IActionResult> Login(
    [FromBody] LoginRequest request)
{
    // Login logic
}

[HttpPost("create-order")]
[EnableRateLimiting("payment")]
public async Task<IActionResult> CreateOrder(
    [FromBody] CreateOrderCommand command)
{
    // Order logic
}

[HttpGet("sales-report")]
[EnableRateLimiting("reports")]
public async Task<IActionResult> GetSalesReport()
{
    // Report logic
}
```

**Per-User Rate Limiting:**
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("per-user", context =>
    {
        var userId = context.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? "anonymous";

        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: userId,
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 50,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4
            });
    });
});
```

### Additional Security Measures

**HTTPS Enforcement:**
```csharp
app.UseHttpsRedirection();
app.UseHsts();  // HTTP Strict Transport Security
```

**Security Headers:**
```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append(
        "Content-Security-Policy",
        "default-src 'self'");
    context.Response.Headers.Append(
        "Referrer-Policy",
        "strict-origin-when-cross-origin");

    await next();
});
```

**CORS Configuration:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        policy.WithOrigins(
            "https://app.example.com",
            "https://admin.example.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

app.UseCors("AllowedOrigins");
```

### Implementation Notes
- Store JWT secret in Azure Key Vault, never in appsettings.json
- Rotate refresh tokens on each use (token rotation pattern)
- Implement token revocation for logout
- Use HTTPS only in production
- Set HttpOnly, Secure, SameSite cookies for web clients
- Monitor failed login attempts; implement account lockout
- Log security events (authentication, authorization failures)
- BCrypt work factor should increase over time (currently 12)
- Rate limit by IP and/or user ID
- Consider implementing 2FA for admin accounts

### Alternatives Considered
- **IdentityServer/Duende**: Too heavy for this use case
- **OAuth2/OIDC**: Not needed for internal POS system
- **Session-based auth**: Doesn't scale well, requires sticky sessions
- **API Keys**: Less secure than JWT for user authentication
- **AspNetCoreRateLimit package**: .NET 9 has built-in support now

---

## 8. Performance Optimization

### Decision
Optimize Entity Framework Core 9 queries using AsNoTracking(), compiled models, projection, pagination strategies, and multi-level caching with Redis.

### Rationale
- **EF Core 9 Performance**: Up to 30% better query performance vs. previous versions
- **Scalability**: Efficient queries handle 50-100 concurrent users per store
- **User Experience**: Sub-second response times for POS operations
- **Cost Efficiency**: Reduced database load lowers infrastructure costs
- **Data Volume**: 7-year retention requires optimized queries

### EF Core 9 Optimizations

**1. AsNoTracking() for Read-Only Queries:**
```csharp
// Poor performance - change tracking overhead
public async Task<List<ProductDto>> GetProductsAsync()
{
    return await _context.Products
        .ToListAsync();  // Tracks all entities
}

// Optimized - no change tracking
public async Task<List<ProductDto>> GetProductsAsync()
{
    return await _context.Products
        .AsNoTracking()
        .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
        .ToListAsync();
}
```

**2. Compiled Queries for Frequently Executed Operations:**
```csharp
public class OrderQueries
{
    // Compiled query - compiled once, executed many times
    private static readonly Func<ApplicationDbContext, Guid, Task<Order>>
        GetOrderByIdCompiled = EF.CompileAsyncQuery(
            (ApplicationDbContext context, Guid id) =>
                context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefault(o => o.Id == id));

    public async Task<Order> GetOrderByIdAsync(Guid id)
    {
        return await GetOrderByIdCompiled(_context, id);
    }
}
```

**3. Compiled Models (.NET 9 Feature):**
```csharp
// Generate compiled model at build time
// Improves startup performance by ~50% for large models

// In .csproj
<PropertyGroup>
    <CompiledModelDirectory>CompiledModels</CompiledModelDirectory>
</PropertyGroup>

// Generate using CLI
// dotnet ef dbcontext optimize

// Use in DbContext
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseModel(CompiledModels.ApplicationDbContextModel.Instance);
}
```

**4. Projection to Avoid Over-Fetching:**
```csharp
// Bad - loads all columns
public async Task<List<ProductDto>> GetProductsAsync()
{
    var products = await _context.Products.ToListAsync();
    return _mapper.Map<List<ProductDto>>(products);
}

// Good - select only needed columns
public async Task<List<ProductDto>> GetProductsAsync()
{
    return await _context.Products
        .Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            StockQuantity = p.StockQuantity
        })
        .ToListAsync();
}
```

**5. Eager Loading vs. Explicit Loading:**
```csharp
// Eager loading - one query with JOIN
public async Task<Order> GetOrderWithItemsAsync(Guid id)
{
    return await _context.Orders
        .Include(o => o.OrderItems)
        .ThenInclude(oi => oi.Product)
        .FirstOrDefaultAsync(o => o.Id == id);
}

// Explicit loading - multiple queries (use when related data rarely needed)
public async Task<Order> GetOrderAsync(Guid id)
{
    var order = await _context.Orders
        .FirstOrDefaultAsync(o => o.Id == id);

    if (order != null && needsItems)
    {
        await _context.Entry(order)
            .Collection(o => o.OrderItems)
            .LoadAsync();
    }

    return order;
}
```

**6. Split Queries for Cartesian Explosion:**
```csharp
// Problem: Cartesian explosion with multiple includes
// Solution: Use AsSplitQuery()
public async Task<List<Order>> GetOrdersWithDetailsAsync()
{
    return await _context.Orders
        .Include(o => o.OrderItems)      // Many
        .Include(o => o.Payments)        // Many
        .Include(o => o.Shipments)       // Many
        .AsSplitQuery()                  // Executes separate queries
        .ToListAsync();
}
```

### Pagination Strategies

**1. Offset-Based Pagination (Traditional):**
```csharp
public async Task<PagedResult<OrderDto>> GetOrdersAsync(
    int pageNumber,
    int pageSize)
{
    var query = _context.Orders.AsNoTracking();

    var totalCount = await query.CountAsync();

    var orders = await query
        .OrderByDescending(o => o.CreatedAt)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
        .ToListAsync();

    return new PagedResult<OrderDto>
    {
        Items = orders,
        TotalCount = totalCount,
        PageNumber = pageNumber,
        PageSize = pageSize,
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
    };
}
```

**2. Keyset Pagination (Cursor-Based) - More Efficient:**
```csharp
public async Task<CursorPagedResult<OrderDto>> GetOrdersAsync(
    DateTime? afterCreatedAt = null,
    Guid? afterId = null,
    int limit = 20)
{
    var query = _context.Orders
        .AsNoTracking()
        .OrderByDescending(o => o.CreatedAt)
        .ThenByDescending(o => o.Id);

    // Apply cursor filter
    if (afterCreatedAt.HasValue && afterId.HasValue)
    {
        query = query.Where(o =>
            o.CreatedAt < afterCreatedAt.Value ||
            (o.CreatedAt == afterCreatedAt.Value && o.Id < afterId.Value));
    }

    var orders = await query
        .Take(limit + 1)  // Fetch one extra to determine if there's more
        .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
        .ToListAsync();

    var hasMore = orders.Count > limit;
    if (hasMore) orders.RemoveAt(limit);

    return new CursorPagedResult<OrderDto>
    {
        Items = orders,
        HasMore = hasMore,
        NextCursor = hasMore
            ? new Cursor
              {
                  CreatedAt = orders.Last().CreatedAt,
                  Id = orders.Last().Id
              }
            : null
    };
}
```

**Comparison:**
- **Offset-Based**: Intuitive, allows jumping to specific pages, but slow for large offsets
- **Keyset**: Consistent performance regardless of offset, ideal for infinite scroll, no random page access

### Multi-Level Caching Strategy

**1. In-Memory Cache (L1) - Application Level:**
```csharp
builder.Services.AddMemoryCache();

public class ProductService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly IApplicationDbContext _context;

    public async Task<Product> GetProductAsync(Guid id)
    {
        var cacheKey = $"product:{id}";

        // L1: In-memory cache (fastest)
        if (_memoryCache.TryGetValue(cacheKey, out Product cachedProduct))
            return cachedProduct;

        // L2: Distributed cache (Redis)
        var redisProduct = await _distributedCache.GetStringAsync(cacheKey);
        if (redisProduct != null)
        {
            var product = JsonSerializer.Deserialize<Product>(redisProduct);

            // Store in L1
            _memoryCache.Set(cacheKey, product, TimeSpan.FromMinutes(5));
            return product;
        }

        // L3: Database
        var dbProduct = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (dbProduct == null) return null;

        // Store in L2
        await _distributedCache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(dbProduct),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });

        // Store in L1
        _memoryCache.Set(cacheKey, dbProduct, TimeSpan.FromMinutes(5));

        return dbProduct;
    }
}
```

**2. Cache-Aside Pattern with MediatR Behavior:**
```csharp
public class CachingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery
{
    private readonly IDistributedCache _cache;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var cacheKey = request.CacheKey;

        // Try cache first
        var cachedResponse = await _cache.GetStringAsync(
            cacheKey,
            cancellationToken);

        if (cachedResponse != null)
        {
            return JsonSerializer.Deserialize<TResponse>(cachedResponse);
        }

        // Execute query
        var response = await next();

        // Store in cache
        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(response),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = request.CacheExpiration
            },
            cancellationToken);

        return response;
    }
}

// Cacheable query interface
public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan CacheExpiration { get; }
}

// Usage
public record GetProductByIdQuery : IRequest<ProductDto>, ICacheableQuery
{
    public Guid Id { get; init; }
    public string CacheKey => $"product:{Id}";
    public TimeSpan CacheExpiration => TimeSpan.FromHours(1);
}
```

**3. Response Caching Middleware:**
```csharp
// Program.cs
builder.Services.AddResponseCaching();
app.UseResponseCaching();

// Controller
[HttpGet("{id}")]
[ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "id" })]
public async Task<IActionResult> GetProduct(Guid id)
{
    var product = await _mediator.Send(new GetProductByIdQuery { Id = id });
    return Ok(product);
}
```

### Database Indexing Strategies

**1. Covering Indexes:**
```csharp
// Migration
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateIndex(
        name: "idx_orders_store_status_covering",
        table: "orders",
        columns: new[] { "store_id", "status", "created_at" },
        includeColumns: new[] { "total_amount", "customer_id" });
}
```

**2. Filtered Indexes:**
```sql
-- Index only active orders
CREATE INDEX idx_orders_active
ON orders (store_id, created_at DESC)
WHERE status IN ('pending', 'processing');
```

**3. Index Usage Monitoring:**
```csharp
// Log generated SQL to identify missing indexes
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder
        .UseNpgsql(connectionString)
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors();
}
```

### Bulk Operations

**EFCore.BulkExtensions for Large Inserts:**
```csharp
// Slow - individual inserts
foreach (var item in orderItems)
{
    await _context.OrderItems.AddAsync(item);
}
await _context.SaveChangesAsync();

// Fast - bulk insert
await _context.BulkInsertAsync(orderItems);
```

### Implementation Notes
- Enable query plan caching in PostgreSQL (set `shared_preload_libraries = 'pg_stat_statements'`)
- Use EXPLAIN ANALYZE to identify slow queries
- Monitor cache hit ratio; adjust TTL if below 80%
- Implement cache warming for frequently accessed data
- Use Redis for distributed cache, not in-memory for scaled deployments
- Prefer keyset pagination for API endpoints
- Use compiled models in production for 50% faster startup
- Index all foreign keys and frequently filtered columns
- Monitor query performance with Application Insights

### Alternatives Considered
- **Dapper for read queries**: Added complexity, EF Core 9 performance is sufficient
- **Materialized views**: Useful but adds maintenance overhead
- **Elasticsearch for search**: Overkill for current scale
- **GraphQL with DataLoader**: Adds complexity, REST is sufficient

---

## 9. Vietnamese Localization

### Decision
Implement localization using .NET resource files (.resx) with request localization middleware supporting Vietnamese (vi-VN) and English (en-US) cultures.

### Rationale
- **Built-in Support**: ASP.NET Core has comprehensive localization features
- **Type Safety**: Resource files provide compile-time checking
- **Maintainability**: Centralized translation management
- **Flexibility**: Easy to add new languages in future
- **Performance**: Resource files are compiled, no runtime overhead

### Localization Configuration

**Program.cs Setup:**
```csharp
// Add localization services
builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});

// Configure supported cultures
var supportedCultures = new[]
{
    new CultureInfo("vi-VN"),  // Vietnamese
    new CultureInfo("en-US")   // English (default)
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("vi-VN");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    // Culture providers (order matters)
    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new QueryStringRequestCultureProvider(),      // ?culture=vi-VN
        new CookieRequestCultureProvider(),           // Cookie
        new AcceptLanguageHeaderRequestCultureProvider()  // Accept-Language header
    };
});

// Apply localization middleware
app.UseRequestLocalization();
```

### Resource File Structure

```
Project/
├── Resources/
│   ├── SharedResources.vi-VN.resx
│   ├── SharedResources.en-US.resx
│   ├── Controllers/
│   │   ├── OrderController.vi-VN.resx
│   │   └── OrderController.en-US.resx
│   └── ValidationMessages/
│       ├── ValidationMessages.vi-VN.resx
│       └── ValidationMessages.en-US.resx
```

**SharedResources.vi-VN.resx:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="Order" xml:space="preserve">
    <value>Đơn hàng</value>
  </data>
  <data name="Product" xml:space="preserve">
    <value>Sản phẩm</value>
  </data>
  <data name="Customer" xml:space="preserve">
    <value>Khách hàng</value>
  </data>
  <data name="TotalAmount" xml:space="preserve">
    <value>Tổng tiền</value>
  </data>
  <data name="OrderCreatedSuccessfully" xml:space="preserve">
    <value>Đơn hàng đã được tạo thành công</value>
  </data>
  <data name="OrderNotFound" xml:space="preserve">
    <value>Không tìm thấy đơn hàng</value>
  </data>
</root>
```

**ValidationMessages.vi-VN.resx:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="Required" xml:space="preserve">
    <value>{0} là bắt buộc</value>
  </data>
  <data name="MinLength" xml:space="preserve">
    <value>{0} phải có ít nhất {1} ký tự</value>
  </data>
  <data name="MaxLength" xml:space="preserve">
    <value>{0} không được vượt quá {1} ký tự</value>
  </data>
  <data name="EmailInvalid" xml:space="preserve">
    <value>Địa chỉ email không hợp lệ</value>
  </data>
  <data name="PhoneInvalid" xml:space="preserve">
    <value>Số điện thoại không hợp lệ</value>
  </data>
</root>
```

### Using Localization in Code

**1. Controllers:**
```csharp
public class OrderController : ControllerBase
{
    private readonly IStringLocalizer<OrderController> _localizer;
    private readonly IStringLocalizer<SharedResources> _sharedLocalizer;

    public OrderController(
        IStringLocalizer<OrderController> localizer,
        IStringLocalizer<SharedResources> sharedLocalizer)
    {
        _localizer = localizer;
        _sharedLocalizer = sharedLocalizer;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderCommand command)
    {
        var result = await _mediator.Send(command);

        return Ok(new
        {
            message = _sharedLocalizer["OrderCreatedSuccessfully"],
            data = result
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var order = await _mediator.Send(new GetOrderByIdQuery { Id = id });

        if (order == null)
        {
            return NotFound(new
            {
                error = _sharedLocalizer["OrderNotFound"]
            });
        }

        return Ok(order);
    }
}
```

**2. FluentValidation with Localization:**
```csharp
public class CreateOrderCommandValidator
    : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator(
        IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .WithMessage(localizer["Required", localizer["Customer"]])
            .MaximumLength(100)
            .WithMessage(localizer["MaxLength", localizer["Customer"], "100"]);

        RuleFor(x => x.CustomerEmail)
            .NotEmpty()
            .WithMessage(localizer["Required", "Email"])
            .EmailAddress()
            .WithMessage(localizer["EmailInvalid"]);

        RuleFor(x => x.CustomerPhone)
            .NotEmpty()
            .WithMessage(localizer["Required", localizer["Phone"]])
            .Matches(@"^(0|\+84)[0-9]{9}$")
            .WithMessage(localizer["PhoneInvalid"]);
    }
}
```

**3. Domain Events with Localization:**
```csharp
public class OrderCreatedEventHandler
    : INotificationHandler<OrderCreatedEvent>
{
    private readonly IStringLocalizer<SharedResources> _localizer;
    private readonly IHubContext<NotificationHub> _hubContext;

    public async Task Handle(
        OrderCreatedEvent notification,
        CancellationToken cancellationToken)
    {
        var message = _localizer[
            "OrderCreatedNotification",
            notification.OrderNumber,
            notification.TotalAmount.ToString("N0")
        ];

        await _hubContext.Clients
            .Group($"Store_{notification.StoreId}")
            .SendAsync("ReceiveNotification", message, cancellationToken);
    }
}
```

### Culture Provider Strategies

**1. Query String Provider:**
```
GET /api/orders?culture=vi-VN
```

**2. Cookie Provider:**
```csharp
[HttpPost("set-language")]
public IActionResult SetLanguage(string culture, string returnUrl)
{
    Response.Cookies.Append(
        CookieRequestCultureProvider.DefaultCookieName,
        CookieRequestCultureProvider.MakeCookieValue(
            new RequestCulture(culture)),
        new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        }
    );

    return LocalRedirect(returnUrl);
}
```

**3. Accept-Language Header:**
```
GET /api/orders
Accept-Language: vi-VN,vi;q=0.9,en-US;q=0.8,en;q=0.7
```

**4. Custom User Preference Provider:**
```csharp
public class UserPreferenceCultureProvider : RequestCultureProvider
{
    public override async Task<ProviderCultureResult> DetermineProviderCultureResult(
        HttpContext httpContext)
    {
        var userId = httpContext.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null) return null;

        // Get culture from user preferences in database
        var userService = httpContext.RequestServices
            .GetRequiredService<IUserService>();

        var userCulture = await userService.GetUserCultureAsync(
            Guid.Parse(userId));

        if (string.IsNullOrEmpty(userCulture))
            return null;

        return new ProviderCultureResult(userCulture);
    }
}

// Register in Program.cs
options.RequestCultureProviders.Insert(0,
    new UserPreferenceCultureProvider());
```

### Number, Currency, and Date Formatting

**1. Currency Formatting:**
```csharp
public class OrderDto
{
    public decimal TotalAmount { get; set; }

    public string FormattedAmount
    {
        get
        {
            var culture = CultureInfo.CurrentCulture;

            // Vietnamese: 100.000 ₫
            // English: $100.00
            return culture.Name switch
            {
                "vi-VN" => $"{TotalAmount:N0} ₫",
                "en-US" => TotalAmount.ToString("C", culture),
                _ => TotalAmount.ToString("N2", culture)
            };
        }
    }
}
```

**2. Date Formatting:**
```csharp
public class OrderDto
{
    public DateTime CreatedAt { get; set; }

    public string FormattedDate
    {
        get
        {
            var culture = CultureInfo.CurrentCulture;

            // Vietnamese: 05/10/2025 14:30
            // English: 10/5/2025 2:30 PM
            return CreatedAt.ToString("g", culture);
        }
    }
}
```

**3. Time Zone Handling:**
```csharp
public class DateTimeService
{
    private static readonly TimeZoneInfo VietnamTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    public DateTime ConvertToVietnamTime(DateTime utcDateTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(
            utcDateTime,
            VietnamTimeZone);
    }

    public DateTime ConvertToUtc(DateTime vietnamDateTime)
    {
        return TimeZoneInfo.ConvertTimeToUtc(
            vietnamDateTime,
            VietnamTimeZone);
    }
}
```

### Database Localization

**For Translatable Content:**
```csharp
public class Product
{
    public Guid Id { get; set; }

    // JSON column for translations
    public Dictionary<string, ProductTranslation> Translations { get; set; }

    public string GetName(string culture)
    {
        if (Translations.TryGetValue(culture, out var translation))
            return translation.Name;

        // Fallback to default culture
        return Translations["vi-VN"].Name;
    }
}

public class ProductTranslation
{
    public string Name { get; set; }
    public string Description { get; set; }
}

// EF Core configuration
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>()
        .Property(p => p.Translations)
        .HasColumnType("jsonb");
}
```

### Implementation Notes
- Store all dates in UTC, convert to Vietnam time zone (UTC+7) for display
- Use Vietnamese Dong (₫) as default currency
- Validate Vietnamese phone numbers: `^(0|\+84)[0-9]{9}$`
- Support Vietnamese characters (Unicode) in all text fields
- Default to Vietnamese (vi-VN) culture for POS operations
- Provide language switcher in admin panel
- Use resource files for UI text, not hardcoded strings
- Consider separate resource files for different modules
- Cache resource strings for performance

### Alternatives Considered
- **Database-driven localization**: More flexible but slower, requires caching
- **JSON files for translations**: No compile-time checking, more error-prone
- **Third-party services (Crowdin, Lokalise)**: Added cost and dependency
- **Separate API per language**: Maintenance nightmare, rejected

---

## 10. Testing Strategy

### Decision
Implement comprehensive testing using Testcontainers for integration tests with PostgreSQL and Redis, and contract testing with OpenAPI specifications in .NET 9.

### Rationale
- **Real Dependencies**: Testcontainers provide actual PostgreSQL/Redis instances
- **Reliability**: Tests run against production-like environment
- **Isolation**: Each test gets fresh containers, no shared state
- **CI/CD Friendly**: Containers start/stop automatically
- **Contract Testing**: OpenAPI ensures API specification matches implementation
- **Confidence**: Catches integration issues early

### Testcontainers Setup

**1. NuGet Packages:**
```xml
<ItemGroup>
  <PackageReference Include="Testcontainers" Version="3.8.0" />
  <PackageReference Include="Testcontainers.PostgreSql" Version="3.8.0" />
  <PackageReference Include="Testcontainers.Redis" Version="3.8.0" />
  <PackageReference Include="xunit" Version="2.8.0" />
  <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
</ItemGroup>
```

**2. Test Infrastructure with IAsyncLifetime:**
```csharp
public class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly RedisContainer _redisContainer;

    public string PostgresConnectionString { get; private set; }
    public string RedisConnectionString { get; private set; }

    public IntegrationTestFixture()
    {
        // PostgreSQL container
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:17")
            .WithDatabase("pos_test")
            .WithUsername("postgres")
            .WithPassword("test_password")
            .WithCleanUp(true)
            .Build();

        // Redis container
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        // Start containers in parallel
        await Task.WhenAll(
            _postgresContainer.StartAsync(),
            _redisContainer.StartAsync()
        );

        PostgresConnectionString = _postgresContainer.GetConnectionString();
        RedisConnectionString = _redisContainer.GetConnectionString();

        // Apply migrations
        await ApplyMigrationsAsync();
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            _postgresContainer.DisposeAsync().AsTask(),
            _redisContainer.DisposeAsync().AsTask()
        );
    }

    private async Task ApplyMigrationsAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(PostgresConnectionString)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.MigrateAsync();
    }
}
```

**3. Custom WebApplicationFactory:**
```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly IntegrationTestFixture _fixture;

    public CustomWebApplicationFactory(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Replace DbContext with test database
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(_fixture.PostgresConnectionString);
            });

            // Replace Redis with test Redis
            services.RemoveAll<IConnectionMultiplexer>();
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configuration = ConfigurationOptions.Parse(
                    _fixture.RedisConnectionString);
                return ConnectionMultiplexer.Connect(configuration);
            });

            // Replace distributed cache
            services.RemoveAll<IDistributedCache>();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = _fixture.RedisConnectionString;
            });
        });
    }
}
```

### Integration Test Examples

**1. Order Creation Test:**
```csharp
public class OrderIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly HttpClient _client;

    public OrderIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        var factory = new CustomWebApplicationFactory(fixture);
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_ReturnsCreatedOrder()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            StoreId = Guid.NewGuid(),
            CustomerName = "Nguyen Van A",
            CustomerEmail = "nguyenvana@example.com",
            CustomerPhone = "0987654321",
            Items = new List<OrderItemDto>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 2, UnitPrice = 100000 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", command);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content
            .ReadFromJsonAsync<OrderDto>();

        Assert.NotNull(result);
        Assert.Equal(command.CustomerName, result.CustomerName);
        Assert.Equal(200000, result.TotalAmount);
    }

    [Fact]
    public async Task CreateOrder_DuplicateRequest_IsIdempotent()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            IdempotencyKey = Guid.NewGuid().ToString(),
            StoreId = Guid.NewGuid(),
            CustomerName = "Nguyen Van B",
            Items = new List<OrderItemDto>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 50000 }
            }
        };

        // Act - Send same request twice
        var response1 = await _client.PostAsJsonAsync("/api/orders", command);
        var response2 = await _client.PostAsJsonAsync("/api/orders", command);

        // Assert
        response1.EnsureSuccessStatusCode();
        response2.EnsureSuccessStatusCode();

        var result1 = await response1.Content.ReadFromJsonAsync<OrderDto>();
        var result2 = await response2.Content.ReadFromJsonAsync<OrderDto>();

        Assert.Equal(result1.Id, result2.Id);
    }
}
```

**2. Caching Integration Test:**
```csharp
public class CachingIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IDistributedCache _cache;
    private readonly ApplicationDbContext _context;

    public CachingIntegrationTests(IntegrationTestFixture fixture)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(fixture.PostgresConnectionString)
            .Options;

        _context = new ApplicationDbContext(options);

        var redis = ConnectionMultiplexer.Connect(
            fixture.RedisConnectionString);
        _cache = new RedisCache(new RedisCacheOptions
        {
            ConnectionMultiplexerFactory = () => Task.FromResult(redis)
        });
    }

    [Fact]
    public async Task GetProduct_FirstCall_LoadsFromDatabase()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Price = 100000
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var cacheKey = $"product:{product.Id}";

        // Act
        var cachedValue = await _cache.GetStringAsync(cacheKey);

        // Assert
        Assert.Null(cachedValue); // Not in cache initially

        // Simulate service call that caches
        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(product));

        var cachedAfter = await _cache.GetStringAsync(cacheKey);
        Assert.NotNull(cachedAfter);
    }

    [Fact]
    public async Task CacheInvalidation_UpdateProduct_RemovesFromCache()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product"
        };
        var cacheKey = $"product:{product.Id}";

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(product));

        // Act
        await _cache.RemoveAsync(cacheKey);
        var result = await _cache.GetStringAsync(cacheKey);

        // Assert
        Assert.Null(result);
    }
}
```

**3. Database Partition Test:**
```csharp
public class PartitioningIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly ApplicationDbContext _context;

    public PartitioningIntegrationTests(IntegrationTestFixture fixture)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(fixture.PostgresConnectionString)
            .Options;

        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task OrderPartitioning_InsertsIntoCorrectPartition()
    {
        // Arrange
        var order2025 = new Order
        {
            Id = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            OrderNumber = "ORD-2025-001",
            TotalAmount = 100000,
            CreatedAt = new DateTime(2025, 10, 5)
        };

        var order2026 = new Order
        {
            Id = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            OrderNumber = "ORD-2026-001",
            TotalAmount = 200000,
            CreatedAt = new DateTime(2026, 1, 1)
        };

        // Act
        await _context.Orders.AddRangeAsync(order2025, order2026);
        await _context.SaveChangesAsync();

        // Assert - Query specific partition
        var sql = @"
            SELECT COUNT(*) FROM orders_2025 WHERE id = @id
        ";
        var count2025 = await _context.Database
            .SqlQueryRaw<int>(sql,
                new NpgsqlParameter("@id", order2025.Id))
            .FirstOrDefaultAsync();

        Assert.Equal(1, count2025);
    }
}
```

### Contract Testing with OpenAPI

**1. OpenAPI Document Generation:**
```csharp
public class OpenApiContractTests : IClassFixture<IntegrationTestFixture>
{
    private readonly HttpClient _client;

    public OpenApiContractTests(IntegrationTestFixture fixture)
    {
        var factory = new CustomWebApplicationFactory(fixture);
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task OpenApiDocument_IsValid()
    {
        // Act
        var response = await _client.GetAsync("/openapi/v1.json");

        // Assert
        response.EnsureSuccessStatusCode();
        var openApiJson = await response.Content.ReadAsStringAsync();

        Assert.NotEmpty(openApiJson);

        // Validate schema
        var openApiDocument = JsonSerializer
            .Deserialize<OpenApiDocument>(openApiJson);

        Assert.NotNull(openApiDocument);
        Assert.NotEmpty(openApiDocument.Paths);
    }

    [Fact]
    public async Task OpenApiDocument_ContainsAllEndpoints()
    {
        // Arrange
        var expectedPaths = new[]
        {
            "/api/orders",
            "/api/orders/{id}",
            "/api/products",
            "/api/products/{id}",
            "/api/auth/login",
            "/api/auth/refresh"
        };

        // Act
        var response = await _client.GetAsync("/openapi/v1.json");
        var openApiJson = await response.Content.ReadAsStringAsync();
        var document = JsonSerializer
            .Deserialize<JsonDocument>(openApiJson);

        var paths = document.RootElement
            .GetProperty("paths")
            .EnumerateObject()
            .Select(p => p.Name)
            .ToList();

        // Assert
        foreach (var expectedPath in expectedPaths)
        {
            Assert.Contains(expectedPath, paths);
        }
    }
}
```

**2. Schema Validation Test:**
```csharp
public class SchemaValidationTests
{
    [Fact]
    public async Task CreateOrderRequest_MatchesOpenApiSchema()
    {
        // Arrange
        var schema = await LoadOpenApiSchemaAsync("CreateOrderCommand");

        var validRequest = new
        {
            storeId = Guid.NewGuid(),
            customerName = "Nguyen Van A",
            customerEmail = "test@example.com",
            customerPhone = "0987654321",
            items = new[]
            {
                new { productId = Guid.NewGuid(), quantity = 1, unitPrice = 100000 }
            }
        };

        var json = JsonSerializer.Serialize(validRequest);

        // Act & Assert
        var isValid = ValidateAgainstSchema(json, schema);
        Assert.True(isValid);
    }

    [Theory]
    [InlineData("")]  // Empty customer name
    [InlineData("invalid-email")]  // Invalid email
    [InlineData("123")]  // Invalid phone
    public async Task CreateOrderRequest_InvalidData_FailsValidation(
        string invalidField)
    {
        // Similar validation test for invalid scenarios
    }
}
```

### Unit Tests Best Practices

**1. Testing MediatR Handlers:**
```csharp
public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _handler = new CreateOrderCommandHandler(
            _contextMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesOrder()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            StoreId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            Items = new List<OrderItemDto>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 100 }
            }
        };

        var dbSetMock = new Mock<DbSet<Order>>();
        _contextMock.Setup(x => x.Orders).Returns(dbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        _contextMock.Verify(
            x => x.Orders.AddAsync(It.IsAny<Order>(), default),
            Times.Once);
        _contextMock.Verify(
            x => x.SaveChangesAsync(default),
            Times.Once);
    }
}
```

### Performance Testing

**1. Load Test with NBomber:**
```csharp
public class LoadTests
{
    [Fact]
    public void CreateOrder_LoadTest_Handles100ConcurrentUsers()
    {
        var scenario = Scenario.Create("create_order", async context =>
        {
            var command = new
            {
                storeId = Guid.NewGuid(),
                customerName = "Load Test Customer",
                items = new[]
                {
                    new { productId = Guid.NewGuid(), quantity = 1, unitPrice = 100 }
                }
            };

            var response = await Http.CreateRequest("POST", "https://localhost:5001/api/orders")
                .WithJsonBody(command)
                .WithHeader("Authorization", "Bearer test-token")
                .SendAsync(context);

            return response;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(10))
        .WithLoadSimulations(
            Simulation.KeepConstant(copies: 100, during: TimeSpan.FromMinutes(1))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        Assert.True(stats.AllOkCount > 5000);
        Assert.True(stats.AllFailCount < 100);
    }
}
```

### CI/CD Integration

**GitHub Actions Workflow:**
```yaml
name: Integration Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v4

    - name: Setup .NET 9
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --no-restore

    - name: Run Integration Tests
      run: dotnet test --no-build --verbosity normal
      env:
        DOCKER_HOST: unix:///var/run/docker.sock
```

### Implementation Notes
- **Pin Container Images**: Use specific versions (postgres:17, redis:7) to avoid surprises
- **Parallel Tests**: Use `[Collection]` attribute to isolate tests that can't run in parallel
- **Dynamic Ports**: Testcontainers assigns random ports; use `GetConnectionString()` or `GetConnectionString()`
- **Cleanup**: Containers auto-cleanup with `WithCleanUp(true)`
- **Shared Fixtures**: Use `IClassFixture<>` for tests sharing setup
- **CI Performance**: Containers start in ~2-5 seconds on modern CI runners
- **Test Data**: Use `Bogus` library to generate realistic test data
- **OpenAPI Sync**: Run contract tests in CI to ensure API matches spec

### Alternatives Considered
- **In-Memory Database**: Doesn't test real PostgreSQL features (partitioning, JSONB, etc.)
- **Shared Test Database**: State pollution between tests, slow cleanup
- **Docker Compose for Tests**: Manual setup, not integrated with test lifecycle
- **Mocking External Dependencies**: Doesn't catch integration issues
- **Manual Contract Testing**: Error-prone, OpenAPI validation is automated

---

## Summary

This research document provides comprehensive technical guidance for building a POS backend API system with:

- **Clean Architecture** for maintainability and testability
- **CQRS with MediatR** for scalable command/query separation
- **PostgreSQL** with multi-tenancy and 7-year partitioned data retention
- **Redis** for distributed caching, sessions, job queuing, and SignalR backplane
- **SignalR** for real-time notifications across scaled instances
- **External Integrations** (VNPAY, GHN, GHTK) with Polly resilience policies
- **Security** via JWT, refresh tokens, RBAC, BCrypt, and rate limiting
- **Performance** optimizations with EF Core 9, pagination, and multi-level caching
- **Vietnamese Localization** using resource files and culture middleware
- **Testing** with Testcontainers and OpenAPI contract validation

Each decision is backed by technical rationale, implementation examples, and consideration of alternatives, providing a solid foundation for the development team to build a robust, scalable, and maintainable POS system.
