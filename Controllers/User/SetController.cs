using System.Security.Claims;
using DB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Models;

namespace ASP_deneme.Controllers;


[ApiController]
[Route("[controller]")]
public class SetController : ControllerBase
{
    private readonly AppDbContext _context;

    public SetController(AppDbContext context)
    {
        _context = context;
    }
    
    [Authorize]
    [HttpPost("name")]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> SetName([FromBody] NameRequest newName)
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
                return Unauthorized(new { message = "Invalid token" });
            }

            var user = await _context.users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new {message = "User not found"});
            }

            user.name = newName.name;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "True",
                user = new
                {
                    id = user.id,
                    name = user.name,
                    surname = user.surname,
                    email = user.email
                }
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
    
    [Authorize]
    [HttpPost("surname")]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> SetSurname([FromBody] SurnameRequest newSurname)
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
                return Unauthorized(new { message = "Invalid token" });
            }

            var user = await _context.users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new {message = "User not found"});
            }

            user.surname = newSurname.surname;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "True",
                user = new
                {
                    id = user.id,
                    name = user.name,
                    surname = user.surname,
                    email = user.email
                }
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

    [Authorize]
    [HttpPost("email")]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> SetEmail([FromBody] EmailRequest newEmail)
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
                return Unauthorized(new { message = "Invalid token" });
            }

            var user = await _context.users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new {message = "User not found"});
            }

            user.email = newEmail.email;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "True",
                user = new
                {
                    id = user.id,
                    name = user.name,
                    surname = user.surname,
                    email = user.email
                }
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