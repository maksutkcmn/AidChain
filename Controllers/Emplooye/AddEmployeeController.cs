using System.Security.Claims;
using ASP_deneme.Services;
using DB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Models;

namespace ASP_deneme.Controllers;

[ApiController]
[Route("[controller]")]
public class AddEmployeeController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IRedisCacheService _cacheService;

    public AddEmployeeController(AppDbContext context, IRedisCacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    [HttpPost]
    [EnableRateLimiting("fixed")]
    [Authorize]
    public async Task<IActionResult> AddEmployee([FromBody] EmplooyeModel emplooye)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new {message = "Invalid Token"});
            }

            emplooye.adminId = userId;

            await _context.employees.AddAsync(emplooye);
            await _context.SaveChangesAsync();

            // Cache invalidation - yeni employee eklendiğinde cache'i temizle
            string cacheKey = $"employees_{userId}";
            await _cacheService.RemoveAsync(cacheKey);

            var response = await _context.employees.FindAsync(emplooye.id);

            return Ok(new
            {
                message = "Employee added",
                response
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Fail",
                error = ex.Message
            });
        }
    }
}