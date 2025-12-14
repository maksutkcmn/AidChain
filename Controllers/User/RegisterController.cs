using DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Models;
using Services;

namespace ASP_deneme.Controllers;


[ApiController]
[Route("[controller]")]
public class RegisterController : ControllerBase
{
    private readonly AppDbContext _context;

    public RegisterController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> Register([FromBody] UserModel user)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(user.email) || 
            string.IsNullOrWhiteSpace(user.password))
            {
                return BadRequest(new { message = "Email and password are required" });
            }

            var existingUser = await _context.users.FirstOrDefaultAsync(u => u.email == user.email);

            if (existingUser != null)
            {
                return BadRequest(new {message = "This email is existing"});
            }
            user.password = PasswordServices.HashPassword(user.password);

            await _context.users.AddAsync(user);
 
            int rowsAffected = await _context.SaveChangesAsync();

            if (rowsAffected == 0)
            {
                return BadRequest(new {message = "User Create Failure"});
            }

            var response = await _context.users.FindAsync(user.id);

            return Ok(new
            {
                message = "User Create Succesfuly",
                response
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