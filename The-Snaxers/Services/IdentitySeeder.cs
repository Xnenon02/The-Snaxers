namespace TheSnaxers.Services
{
    public interface IIdentitySeeder
    {
        Task SeedAdminAndRolesAsync();
    }

    public class IdentitySeeder : IIdentitySeeder
    {
        // Vi skriver ut Microsoft.AspNetCore.Identity. här:
        private readonly Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser> _userManager;
        private readonly Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public IdentitySeeder(
            Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser> userManager, 
            Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole> _roleManager, 
            IConfiguration configuration)
        {
            _userManager = userManager;
            this._roleManager = _roleManager;
            _configuration = configuration;
        }

        public async Task SeedAdminAndRolesAsync()
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole("Admin"));
            }

            var adminEmail = _configuration["AdminSettings:Email"];
            var adminPassword = _configuration["AdminSettings:Password"];

            if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword))
            {
                var adminUser = await _userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    // Här skriver vi också ut hela namnet:
                    adminUser = new Microsoft.AspNetCore.Identity.IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                    var result = await _userManager.CreateAsync(adminUser, adminPassword);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }
            }
        }
    }
}