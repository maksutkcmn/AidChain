namespace Services
{
	using System;
	using System.IdentityModel.Tokens.Jwt;
	using System.Security.Claims;
	using System.Text;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens;

	public interface IJwtService
    {
        string GenerateToken(int userId, string userName);
    }

	public class JwtService : IJwtService
	{
		private readonly IConfiguration _configuration;

		public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

		public string GenerateToken(int userId, string userName)
        {
			var jwtKey = _configuration["Jwt:Key"];
			if (string.IsNullOrEmpty(jwtKey))
			{
			    throw new InvalidOperationException("JWT Key is not configured");
			}

			var key = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(jwtKey));

			var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

			var token = new JwtSecurityToken(
				issuer: _configuration["Jwt:Issuer"],
				audience: _configuration["Jwt:Audience"],
				claims: claims,
				expires: DateTime.Now.AddHours(24),
				signingCredentials: credentials
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
        }
	}
}