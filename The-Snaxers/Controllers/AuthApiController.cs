using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TheSnaxers.DTOs;

namespace TheSnaxers.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthApiController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthApiController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            // Hämtar din admin-mail direkt från dina User Secrets som Tom fixade igår!
            var adminEmail = _configuration["AdminSettings:Email"];
            
            if (string.IsNullOrEmpty(adminEmail))
            {
                return BadRequest(new { message = "AdminSettings:Email saknas i dina User Secrets!" });
            }

            var user = await _userManager.FindByEmailAsync(adminEmail);
            if (user != null)
            {
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                    new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? string.Empty));

                var token = new SecurityTokenDescriptor
                {
                    Issuer = _configuration["Jwt:Issuer"] ?? "TheSnaxersAPI",
                    Audience = _configuration["Jwt:Audience"] ?? "TheSnaxersApp",
                    Expires = DateTime.Now.AddHours(3),
                    SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256Signature),
                    Subject = new ClaimsIdentity(authClaims)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenObject = tokenHandler.CreateToken(token);

                return Ok(new
                {
                    token = tokenHandler.WriteToken(tokenObject),
                    expiration = token.Expires
                });
            }

            return Unauthorized(new { message = $"Hittade ingen användare i databasen med mailen: {adminEmail}" });
        }
    }
}