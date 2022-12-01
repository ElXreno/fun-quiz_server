using API.Entities.Addons;
using Microsoft.AspNetCore.Identity;

namespace API.Data;

internal interface IDataSeeder
{
    Task SeedData();
    Task SeedRoles();
    Task CreateRolesAsync(params IdentityRole[] roles);
}
public class DataSeeder : IDataSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public DataSeeder(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task SeedData()
    {
        await SeedRoles();
    }

    public async Task SeedRoles()
    {
        await CreateRolesAsync(
            new IdentityRole(UserRoles.Admin),
            new IdentityRole(UserRoles.User),
            new IdentityRole(UserRoles.Moderator)
        );
    }

    // Create multiple Roles if they do not exist
    public async Task CreateRolesAsync(params IdentityRole[] roles)
    {
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role.Name))
            {
                await _roleManager.CreateAsync(role);
            }
        }
    }
}