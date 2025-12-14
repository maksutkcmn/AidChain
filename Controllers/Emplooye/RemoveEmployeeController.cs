using System.Security.Claims;
using ASP_deneme.Services;
using DB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace ASP_deneme.Controllers;

[ApiController]
[Route("[controller]")]
public class RemoveEmployeeController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IRedisCacheService _cacheService;

    public RemoveEmployeeController(AppDbContext context, IRedisCacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    [HttpDelete("/employees/{id:int}")]
    [EnableRateLimiting("fixed")]
    [Authorize]
    public async Task<IActionResult> RemoveEmployee(int id)
    {
        try
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(adminIdClaim) || !int.TryParse(adminIdClaim, out int adminId))
            {
                return Unauthorized(new {message = "Invalid Token"});
            }

            var emplooye = await _context.employees
                            .FirstOrDefaultAsync(e => e.adminId == adminId && e.id == id);

            if (emplooye == null)
            {
                return NotFound(new {message = "Employee not found"});
            }

            _context.employees.Remove(emplooye);
            await _context.SaveChangesAsync();

            // Cache invalidation - employee silindiğinde cache'i temizle
            string cacheKey = $"employees_{adminId}";
            await _cacheService.RemoveAsync(cacheKey);

            return Ok(new
            {
                message = "Employee Deleted",
                name = emplooye.FullName
            });
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Fail",
                error = ex.Message
            });
        }
    }
}