using GamingHub.Api.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace GamingHub.Api.Auth
{
    public class AuthSeeder
    {
        private readonly UserManager<GamingHubUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthSeeder(UserManager<GamingHubUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task SeedAsync()
        {
            await AddRoles();
            await AddAdmin();
        }

        private async Task AddRoles()
        {
            foreach (var role in GamingHubRoles.All)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

        }

        private async Task AddAdmin()
        {
            var userName = _configuration["Admin:UserName"];
            var password = _configuration["Admin:Password"];
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password)) return;
            if (await _userManager.FindByNameAsync(userName) != null) return;

            var admin = new GamingHubUser { UserName = userName, Email = _configuration["Admin:Email"] };
            var result = await _userManager.CreateAsync(admin, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Admin seed failed:" + string.Join("; ", result.Errors.Select(e => e.Description)));
            }
            await _userManager.AddToRolesAsync(admin, GamingHubRoles.All);
        }
    }
}
