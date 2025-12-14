using DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace ASP_deneme.Controllers.Health;

public class ServiceHealthStatus
{
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ResponseTime { get; set; }
    public string? Error { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class HealthCheckController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IDistributedCache _cache;

    public HealthCheckController(AppDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new
        {
            status = "healthy",
            message = "API is running",
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetHealthStatus()
    {
        var databaseHealth = await CheckDatabaseHealth();
        var redisHealth = await CheckRedisHealth();

        var healthStatus = new
        {
            api = "healthy",
            timestamp = DateTime.UtcNow,
            services = new
            {
                database = databaseHealth,
                redis = redisHealth
            }
        };

        bool allHealthy = databaseHealth.Status == "healthy"
                         && redisHealth.Status == "healthy";

        return allHealthy
            ? Ok(healthStatus)
            : StatusCode(503, healthStatus);
    }

    private async Task<ServiceHealthStatus> CheckDatabaseHealth()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();

            if (canConnect)
            {
                return new ServiceHealthStatus
                {
                    Status = "healthy",
                    Message = "Database connection successful",
                    ResponseTime = await MeasureDatabaseResponseTime()
                };
            }

            return new ServiceHealthStatus
            {
                Status = "unhealthy",
                Message = "Cannot connect to database"
            };
        }
        catch (Exception ex)
        {
            return new ServiceHealthStatus
            {
                Status = "unhealthy",
                Message = "Database connection failed",
                Error = ex.Message
            };
        }
    }

    private async Task<ServiceHealthStatus> CheckRedisHealth()
    {
        try
        {
            string testKey = "health_check_test";
            string testValue = DateTime.UtcNow.ToString();

            // Redis'e yazma testi
            await _cache.SetStringAsync(testKey, testValue, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
            });

            // Redis'ten okuma testi
            var cachedValue = await _cache.GetStringAsync(testKey);

            if (cachedValue == testValue)
            {
                // Test verisini temizle
                await _cache.RemoveAsync(testKey);

                return new ServiceHealthStatus
                {
                    Status = "healthy",
                    Message = "Redis connection successful"
                };
            }

            return new ServiceHealthStatus
            {
                Status = "unhealthy",
                Message = "Redis read/write test failed"
            };
        }
        catch (Exception ex)
        {
            return new ServiceHealthStatus
            {
                Status = "unhealthy",
                Message = "Redis connection failed",
                Error = ex.Message
            };
        }
    }

    private async Task<string> MeasureDatabaseResponseTime()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            await _context.Database.ExecuteSqlRawAsync("SELECT 1");
            stopwatch.Stop();
            return $"{stopwatch.ElapsedMilliseconds}ms";
        }
        catch
        {
            return "N/A";
        }
    }
}
