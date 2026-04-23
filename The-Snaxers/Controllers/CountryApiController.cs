using Microsoft.AspNetCore.Mvc;
using TheSnaxers.Services;

[Route("api/[controller]")]
[ApiController]
public class CountryController : ControllerBase
{
    [HttpGet("{code}")]
public IActionResult GetCountryInfo(string code)
{
    // Vi anropar din helper med rätt namn
    var result = CountryFactHelper.GetCountryDetails(code);
    
    // Vi paketerar om till ett snyggt JSON-objekt som JS förstår
    return Ok(new { 
        name = result.CommonName, 
        fact = result.Fact, 
        flag = result.FlagCode 
    });
}
}