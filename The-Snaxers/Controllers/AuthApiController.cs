using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TheSnaxers.DTOs;

namespace TheSnaxers.Controllers;

[ApiController]
[Route("api/v1/auth")] // 🔵 Minor: Konsekvent routing (api/v1/auth istället för api/[controller])
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
        // 🔵 Minor: Validera modellen direkt (om [Required] lagts till på DTO)
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 🔴 Blocker 1: Sök på den inskickade mailen, INTE den hårdkodade admin-mailen
        var user = await _userManager.FindByEmailAsync(model.Email);
        
        // 🔴 Blocker 1: Kontrollera FAKTISKT att lösenordet stämmer!
        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
        {
            // Hämta användarens roller från Identity för att lägga till i JWT
            var roles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // 🔵 Minor: Lägg till rollerna i din token så att [Authorize(Roles = "Admin")] fungerar på API-nivå också!
            foreach (var role in roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            // 🟡 Bör fixas 6: Kasta ett ordentligt exception om nyckeln saknas istället för osäker fallback
            var secretKey = _configuration["Jwt:Secret"] 
                ?? throw new InvalidOperationException("JWT Secret Key is missing in configuration.");

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var token = new SecurityTokenDescriptor
            {
                Issuer = _configuration["Jwt:Issuer"] ?? "TheSnaxersAPI",
                Audience = _configuration["Jwt:Audience"] ?? "TheSnaxersApp",
                Expires = DateTime.UtcNow.AddHours(3), // 🟡 Bör fixas 4: Ändrat från DateTime.Now till UtcCow!
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

        // 🟡 Bör fixas 5: Generiskt felmeddelande som inte läcker ut e-postadresser eller systeminfo
        return Unauthorized(new { message = "Ogiltiga inloggningsuppgifter." });
    }
}