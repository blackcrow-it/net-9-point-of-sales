using System.Text.Json;
using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using StackExchange.Redis;

namespace Infrastructure.Services.Caching;

/// <summary>
/// Redis distributed cache service implementation with retry logic
/// </summary>
public class RedisCacheService : IRedisCacheService, IDisposable
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly ResiliencePipeline _retryPipeline;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(
        IConnectionMultiplexer connectionMultiplexer,
        ILogger<RedisCacheService> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _database = _connectionMultiplexer.GetDatabase();
        _logger = logger;

        // Configure JSON serialization options
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // Configure retry policy with exponential backoff
        _retryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(100),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    _logger.LogWarning(
                        "Redis operation failed. Attempt {Attempt}. Retrying after {Delay}ms. Exception: {Exception}",
                        args.AttemptNumber,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            return await _retryPipeline.ExecuteAsync(async ct =>
            {
                var value = await _database.StringGetAsync(key);
                if (!value.HasValue)
                {
                    _logger.LogDebug("Cache miss for key: {Key}", key);
                    return null;
                }

                _logger.LogDebug("Cache hit for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from Redis cache for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            await _retryPipeline.ExecuteAsync(async ct =>
            {
                var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
                await _database.StringSetAsync(key, serializedValue, expiration);
                _logger.LogDebug("Value set in cache for key: {Key}, Expiration: {Expiration}", key, expiration);
                return ValueTask.CompletedTask;
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in Redis cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _retryPipeline.ExecuteAsync(async ct =>
            {
                var removed = await _database.KeyDeleteAsync(key);
                _logger.LogDebug("Key removed from cache: {Key}, Success: {Success}", key, removed);
                return ValueTask.CompletedTask;
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from Redis cache for key: {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _retryPipeline.ExecuteAsync(async ct =>
            {
                return await _database.KeyExistsAsync(key);
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if key exists in Redis cache: {Key}", key);
            return false;
        }
    }

    public async Task<Dictionary<string, string>> GetCacheInfoAsync()
    {
        var info = new Dictionary<string, string>();

        try
        {
            var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
            var serverInfo = await server.InfoAsync();

            info["IsConnected"] = _connectionMultiplexer.IsConnected.ToString();
            info["ClientName"] = _connectionMultiplexer.ClientName;
            info["Configuration"] = _connectionMultiplexer.Configuration;

            foreach (var section in serverInfo)
            {
                foreach (var item in section)
                {
                    if (item.Key == "redis_version" || item.Key == "used_memory_human" || item.Key == "connected_clients")
                    {
                        info[item.Key] = item.Value;
                    }
                }
            }

            _logger.LogDebug("Retrieved Redis cache info");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Redis cache info");
            info["Error"] = ex.Message;
        }

        return info;
    }

    public void Dispose()
    {
        // ConnectionMultiplexer is managed by DI container, don't dispose here
        GC.SuppressFinalize(this);
    }
}
