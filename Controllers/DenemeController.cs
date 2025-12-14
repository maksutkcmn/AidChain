using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace ASP_deneme.Controllers
{
	public class UserInput
	{
		[Required(ErrorMessage = "İsim boş olamaz")]
		public string? Name {get; set;}

		[Required(ErrorMessage = "Soyisim boş olamaz")]
		public string? Surname {get; set;}
	}
	[ApiController]
	[Route("[controller]")]
	public class DenemeController : ControllerBase
	{

		private readonly IConfiguration _configuration;

		public DenemeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
		[HttpGet("{name}/{surname}")]
		public IActionResult Get(string name, string surname)
		{

			var result = new
			{
				FullName = $"{name} {surname}"
			};

			return Ok(result);
		}

		[HttpPost("api")]
		public IActionResult Post([FromBody] UserInput input)
		{

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			//var result = new
			//{
			//	FullName = $"{input.Name} {input.Surname}"
			//};

			return Ok(input);
		}
		[HttpGet("hop")]
		public IActionResult hop()
		{
			
			var apiKey = _configuration["Jwt:Key"];

			return Ok(apiKey);
		}
	}
}