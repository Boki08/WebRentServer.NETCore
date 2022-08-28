using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using WebRentServer.NETCore.Authentication;
using WebRentServer.NETCore.Models.Entities;
using System.Runtime.Intrinsics.X86;

namespace WebRentServer.NETCore.Persistance
{
    public class DbDataConfiguration:IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        public DbDataConfiguration(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var serviceScope = _serviceProvider.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<RVDBContext>();

                List<string> roles = new List<string> { UserRoles.User, UserRoles.Manager, UserRoles.Admin };

                foreach (string role in roles)
                {
                    var roleStore = new RoleStore<IdentityRole>(context);

                    if (!context.Roles.Any(r => r.Name == role))
                    {
                        await roleStore.CreateAsync(new IdentityRole(role));
                    }
                }

                List<AppUser> users = new List<AppUser> {
                    new AppUser() { FullName = "Admin Adminovic", Email = "admin@yahoo.com", BirthDate = DateTime.Parse("1/1/2000"), DocumentPicture = "123" },
                    new AppUser() { FullName = "Manager", Email = "bojan.vasilic95@gmail.com", BirthDate = DateTime.Parse("1/1/2000"), DocumentPicture = "123" },
                    new AppUser() { FullName = "AppUser AppUserovic", Email = "user@yahoo.com", BirthDate = DateTime.Parse("1/1/2001"), DocumentPicture = "11" }
                };

                var identityUser = new RAIdentityUser();
                var userStore = new UserStore<RAIdentityUser>(context);
                foreach (AppUser user in users)
                {
                    if (!context.AppUsers.Any(u => u.Email == user.Email))
                    {
                        await context.AppUsers.AddAsync(user);
                        await context.SaveChangesAsync();
                    }
                    
                    if (!context.Users.Any(u => u.Email == user.Email))
                    {
                        if (user.Email == "admin@yahoo.com")
                        {
                            var _appUser = context.AppUsers.FirstOrDefault(a => a.FullName == "Admin Adminovic");
                            identityUser = new RAIdentityUser() { Id = "admin", UserName = "admin", Email = _appUser.Email, PasswordHash = RAIdentityUser.HashPassword("admin"), AppUserId = _appUser.UserId };
                        }
                        else if (user.Email == "bojan.vasilic95@gmail.com")
                        {
                            var _appUser = context.AppUsers.FirstOrDefault(a => a.FullName == "Manager");
                            identityUser = new RAIdentityUser() { Id = "manager", UserName = "manager", Email = _appUser.Email, PasswordHash = RAIdentityUser.HashPassword("manager"), AppUserId = _appUser.UserId };
                        }
                        else if (user.Email == "user@yahoo.com")
                        {
                            var _appUser = context.AppUsers.FirstOrDefault(a => a.FullName == "AppUser AppUserovic");
                            identityUser = new RAIdentityUser() { Id = "appu", UserName = "appu", Email = _appUser.Email, PasswordHash = RAIdentityUser.HashPassword("appu"), AppUserId = _appUser.UserId };
                        }

                        var result = await userStore.CreateAsync(identityUser);

                        AssignRoles(_serviceProvider, identityUser.Email, new List<string>() { UserRoles.Admin });
                    }
                }
            }
        }

        public static async Task<IdentityResult> AssignRoles(IServiceProvider services, string email, List<string> roles)
        {
            UserManager<RAIdentityUser> _userManager = services.GetService<UserManager<RAIdentityUser>>();
            RAIdentityUser user = await _userManager.FindByEmailAsync(email);
            var result = await _userManager.AddToRolesAsync(user, roles);

            return result;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}