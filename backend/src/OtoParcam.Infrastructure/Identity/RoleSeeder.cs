using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OtoParcam.Domain.Constants;

namespace OtoParcam.Infrastructure.Identity;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        foreach (var role in new[] { Roles.Administrator, Roles.Customer })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }
}
