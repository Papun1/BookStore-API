using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_API.Data
{
    public static class SeedData
    {
        public async static Task Seed(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await SeedUsers(userManager);
            await SeedRoles(roleManager);

        }
        private async static Task SeedUsers(UserManager<IdentityUser> userManager)
        {
            if(await userManager.FindByEmailAsync("papunsahoo12@gmail.com")== null)
            {
                var user = new IdentityUser
                {
                   UserName="Papun1",
                   Email= "papunsahoo12@gmail.com"
                };
               var result= await userManager.CreateAsync(user,"Passw@rd1");
                if(result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Administrator");
                }
            }
            if (await userManager.FindByEmailAsync("papunsahoo2012@gmail.com") == null)
            {
                var user = new IdentityUser
                {
                    UserName = "Customer1",
                    Email = "papunsahoo2012@gmail.com"
                };
                var result = await userManager.CreateAsync(user, "Password1@");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Customer");
                }
            }
            if (await userManager.FindByEmailAsync("papunsahoo2013@gmail.com") == null)
            {
                var user = new IdentityUser
                {
                    UserName = "Customer2",
                    Email = "papunsahoo2013@gmail.com"
                };
                var result = await userManager.CreateAsync(user, "Password2@");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Customer");
                }
            }
        }
        private async static Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("Administrator"))
            {
                var role = new IdentityRole
                {
                    Name = "Administrator"
                };
                await roleManager.CreateAsync(role);
            }
            if (!await roleManager.RoleExistsAsync("Customer"))
            {
                var role = new IdentityRole
                {
                    Name = "Customer"
                };
                await roleManager.CreateAsync(role);
            }
        }


    }
}
