using GymManager.Api.Infrastructure.Configurations;
using Microsoft.AspNetCore.Identity;

namespace GymManager.Api.Infrastructure.Data.Seeds
{
    public class IdentitySeedService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public IdentitySeedService(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedAsync()
        {
            const string roleName = "OwnerAdmin";
            const string email = "owner@gymmanager.com";
            const string password = "Admin1234";

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
            {
                user = new AppUser
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    Email = email,
                    Nombre = "Owner",
                    Activo = true,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(" | ", result.Errors.Select(x => x.Description));
                    throw new Exception(errors);
                }

                await _userManager.AddToRoleAsync(user, roleName);
            }
        }
    }
}