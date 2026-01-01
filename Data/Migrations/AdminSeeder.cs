using Microsoft.AspNetCore.Identity;

namespace XYZUniversityPortal.Data
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var adminEmail = "admin1@xyz.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
