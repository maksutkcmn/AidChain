using System.Security.Claims;
using ASP_deneme.Services;
using DB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Models;

namespace ASP_deneme.Controllers;

[ApiController]
[Route("[controller]")]
public class GetController : ControllerBase
{
    private readonly IRedisCacheService _cacheService;
    private readonly AppDbContext _context;

    public GetController(IRedisCacheService cacheService, AppDbContext context)
    {
        _context = context;
        _cacheService = cacheService;
    }

    [HttpGet("employees")]
    //[EnableRateLimiting("fixed")]
    //[Authorize]
    public async Task<IActionResult> GetEmployee()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int adminId))
            {
                return Unauthorized(new {message = "Invalid Token"});
            }

            string cacheKey = $"employees_{adminId}";

            // Cache'den veri al
            var employees = await _cacheService.GetAsync<List<EmployeeResponseDto>>(cacheKey);

            // Cache'de yoksa veritabanından al ve cache'e kaydet
            if (employees == null)
            {
                employees = await _context.employees
                .Where(e => e.adminId == adminId)
                .Select(e => new EmployeeResponseDto
                {
                    FullName = e.FullName,
                    Department = e.Department,
                    Position = e.Position,
                    Email = e.Email,
                    Phone = e.Phone,
                    Salary = e.Salary,
                }).ToListAsync();

                if (employees.Any())
                {
                    // 5 dakika cache süresi
                    await _cacheService.SetAsync(cacheKey, employees, TimeSpan.FromMinutes(5));
                }
            }

            if (!employees.Any())
            {
                return NotFound(new {message = "No emplooyes found"});
            }
            
            return Ok(new
            {
                message = "Success",
                count = employees.Count,
                employees
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

    [HttpGet("employees/by-name/{name}")]
    [EnableRateLimiting("fixed")]
    [Authorize]
    public async Task<IActionResult> GetEmployeeByName(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new {message = "Name is required"});
            }

            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(adminIdClaim) || !int.TryParse(adminIdClaim, out int adminId))
            {
                return Unauthorized(new {message = "Invalid Token"});
            }

            var emplooye = await _context.employees
                        .FirstOrDefaultAsync(e => e.adminId == adminId && e.FullName == name);
            
            if (emplooye == null)
            {
                return BadRequest(new {message = "Employee not Found"});
            }

            var response = new EmployeeResponseDto
            {
                FullName = emplooye.FullName,
                Department = emplooye.Department,
                Email = emplooye.Email,
                Position = emplooye.Position,
                Phone = emplooye.Phone,
                Salary = emplooye.Salary
            };

            return Ok(response);
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

    [HttpGet("employees/search")]
    [Authorize]
    public async Task<IActionResult> GetEmployeeSearch([FromQuery] string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new {message = "Bad Input!"});
            }

            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(adminIdClaim) || !int.TryParse(adminIdClaim, out int adminId))
            {
                return Unauthorized(new {message = "Invalid Token"});
            }

            var emplooyes = await _context.employees
                .Where(e => e.adminId == adminId && e.FullName!.ToLower().StartsWith(name.ToLower()))
                .Select(e => e.FullName).ToListAsync();

            if (!emplooyes.Any())
            {
                return Ok(new
                {
                    message = "No employees found",
                    count = 0,
                    result = new List<string>()
                });
            }
            else
            {
                return Ok(new
                {
                    message = "Success",
                    count = emplooyes.Count(),
                    result = emplooyes
                });
            }
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