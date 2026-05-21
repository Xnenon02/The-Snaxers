using System.ComponentModel.DataAnnotations;

namespace TheSnaxers.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "E-postadress måste anges.")]
    [EmailAddress(ErrorMessage = "Ogiltigt e-postformat.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lösenord måste anges.")]
    public string Password { get; set; } = string.Empty;
}