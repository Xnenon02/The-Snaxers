using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TheSnaxers.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ExternalLoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<ExternalLoginModel> _logger;

    public ExternalLoginModel(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        ILogger<ExternalLoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ProviderDisplayName { get; set; }
    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public IActionResult OnGet() => RedirectToPage("./Login");

    public IActionResult OnPost(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
    {
        returnUrl ??= Url.Content("~/");

        if (remoteError != null)
        {
            ErrorMessage = $"Fel från extern leverantör: {remoteError}";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ErrorMessage = "Fel vid hämtning av extern inloggningsinformation.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        // Försök logga in med den externa inloggningen
        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("{Name} loggade in med {LoginProvider}.", info.Principal.Identity?.Name, info.LoginProvider);

            // AC5: Spara/uppdatera profilbild från Google-anspråk och uppdatera cookie
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user != null)
            {
                await SyncGoogleClaimsAsync(user, info);
                await _signInManager.RefreshSignInAsync(user);
            }

            return LocalRedirect(returnUrl);
        }

        if (result.IsLockedOut)
            return RedirectToPage("./Lockout");

        // AC2: Ny användare — skapa konto automatiskt med e-post från Google
        ReturnUrl = returnUrl;
        ProviderDisplayName = info.ProviderDisplayName;

        var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        Input = new InputModel { Email = email };

        // Om vi har e-post från Google kan vi registrera direkt utan extra steg
        if (!string.IsNullOrEmpty(email))
            return await CreateUserAndSignInAsync(info, email, returnUrl);

        return Page();
    }

    public async Task<IActionResult> OnPostConfirmationAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ErrorMessage = "Fel vid hämtning av extern inloggningsinformation vid bekräftelse.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        if (ModelState.IsValid)
            return await CreateUserAndSignInAsync(info, Input.Email, returnUrl);

        ProviderDisplayName = info.ProviderDisplayName;
        ReturnUrl = returnUrl;
        return Page();
    }

    // ===================================================
    // AC2: Skapa profil vid första inloggning
    // OBS: Profilen sparas i SQLite via ASP.NET Core Identity — detta är ett
    // medvetet arkitekturbeslut. CosmosDB används för Favorites (med Identity-användarens ID).
    // ===================================================
    private async Task<IActionResult> CreateUserAndSignInAsync(
        ExternalLoginInfo info, string email, string returnUrl)
    {
        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true   // Google har redan verifierat e-posten
        };

        var createResult = await _userManager.CreateAsync(user);
        if (createResult.Succeeded)
        {
            createResult = await _userManager.AddLoginAsync(user, info);
            if (createResult.Succeeded)
            {
                _logger.LogInformation("Användare {Email} skapad via {Provider}.", email, info.LoginProvider);

                // AC5: Spara profilbild och namn från Google som anspråk
                await SyncGoogleClaimsAsync(user, info);

                await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
        }

        foreach (var error in createResult.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        ProviderDisplayName = info.ProviderDisplayName;
        ReturnUrl = returnUrl;
        return Page();
    }

    // ===================================================
    // AC5: Synkronisera Google-anspråk (namn + profilbild)
    // ===================================================
    private async Task SyncGoogleClaimsAsync(IdentityUser user, ExternalLoginInfo info)
    {
        var existingClaims = await _userManager.GetClaimsAsync(user);

        // Namn
        var nameClaim = info.Principal.FindFirst(ClaimTypes.Name);
        if (nameClaim != null)
        {
            var existing = existingClaims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (existing != null) await _userManager.ReplaceClaimAsync(user, existing, nameClaim);
            else await _userManager.AddClaimAsync(user, nameClaim);
        }

        // Profilbild (Google skickar den som "picture"-anspråk)
        var pictureClaim = info.Principal.FindFirst("picture");
        if (pictureClaim != null)
        {
            var existing = existingClaims.FirstOrDefault(c => c.Type == "picture");
            if (existing != null) await _userManager.ReplaceClaimAsync(user, existing, pictureClaim);
            else await _userManager.AddClaimAsync(user, pictureClaim);
        }
    }
}
