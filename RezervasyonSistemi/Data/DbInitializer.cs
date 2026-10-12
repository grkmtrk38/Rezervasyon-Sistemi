using Microsoft.AspNetCore.Identity;
using RezervasyonSistemi.Models;

namespace RezervasyonSistemi.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Ensure DB is created
            await context.Database.EnsureCreatedAsync();

            // Seed Roles
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed Default Admin User
            string adminEmail = "admin@rezervasyon.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Ad = "Sistem",
                    Soyad = "Yöneticisi",
                    Birim = "Bilgi İşlem",
                    EmailConfirmed = true
                };

                var createAdminResult = await userManager.CreateAsync(adminUser, "Admin123!");
                if (createAdminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed Default Standard User
            string userEmail = "user@rezervasyon.com";
            var standardUser = await userManager.FindByEmailAsync(userEmail);
            if (standardUser == null)
            {
                standardUser = new ApplicationUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    Ad = "Ahmet",
                    Soyad = "Yılmaz",
                    Birim = "Operasyon",
                    EmailConfirmed = true
                };

                var createUserResult = await userManager.CreateAsync(standardUser, "User123!");
                if (createUserResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(standardUser, "User");
                }
            }
        }
    }
}
