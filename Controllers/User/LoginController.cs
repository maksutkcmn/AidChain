using DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Models;
using Services;

namespace ASP_deneme.Controllers;

[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwt;

    public LoginController(AppDbContext context, IJwtService jwt)
    {
        _context = context;
        _jwt = jwt;
    }

    [HttpPost]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> Login([FromBody] LoginModel userLogin)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(userLogin.email) || 
            string.IsNullOrWhiteSpace(userLogin.password))
            {
                return BadRequest(new { message = "Email and password are required" });
            }

            var user = await _context.users.FirstOrDefaultAsync(u => u.email == userLogin.email);

            if (user == null)
            {
                return BadRequest(new {message = "Email or Password dont match"});
            }

            if (string.IsNullOrWhiteSpace(user.email) || 
            string.IsNullOrWhiteSpace(user.password) ||
            string.IsNullOrWhiteSpace(user.name))
            {
                return BadRequest(new { message = "Email and password are required" });
            }

            var validPassword = PasswordServices.VerifyPassword(userLogin.password, user.password);

            if (!validPassword)
            {
                return BadRequest(new {message = "Email or Password dont match"});
            }

            var token = _jwt.GenerateToken(user.id, user.name);
            return Ok(new
            {
                message = "Login Succesfuly",
                token,
                name = user.name,
                surname = user.surname,
                email = user.email,
            });
        }
        catch (System.Exception e)
        {
            return StatusCode(500, new
            {
                message =  "Login Failed",
                error = e.Message
            });
        }
    }
}