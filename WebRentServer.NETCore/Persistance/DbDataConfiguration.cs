using Microsoft.AspNetCore.Identity;
using WebRentServer.NETCore.Authentication;
using WebRentServer.NETCore.Models.Entities;


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

                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<RAIdentityUser>>();
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                List<string> roles = new List<string> { UserRoles.User, UserRoles.Manager, UserRoles.Admin };
                
                foreach (string role in roles)
                {
                    if (!roleManager.Roles.Any(r => r.Name == role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                List<AppUser> users = new List<AppUser> {
                    new AppUser() { FullName = "Admin Adminovic", Email = "admin@yahoo.com", BirthDate = DateTime.Parse("1/1/2000"), DocumentPicture = "123" },
                    new AppUser() { FullName = "Manager", Email = "bojan.vasilic95@gmail.com", BirthDate = DateTime.Parse("1/1/2000"), DocumentPicture = "123" },
                    new AppUser() { FullName = "AppUser AppUserovic", Email = "user@yahoo.com", BirthDate = DateTime.Parse("1/1/2001"), DocumentPicture = "11" }
                };

                var identityUser = new RAIdentityUser();
                string userRole = UserRoles.User;
                foreach (AppUser user in users)
                {
                    if (!context.AppUsers.Any(u => u.Email == user.Email))
                    {
                        await context.AppUsers.AddAsync(user);
                        await context.SaveChangesAsync();
                    }
                    
                    if (!userManager.Users.Any(u => u.Email == user.Email))
                    {
                        if (user.Email == "admin@yahoo.com")
                        {
                            //var _appUser = context.AppUsers.FirstOrDefault(a => a.FullName == "Admin Adminovic");
                            identityUser = new RAIdentityUser() { Id = Guid.NewGuid().ToString(), UserName = "admin", Email = user.Email, PasswordHash = RAIdentityUser.HashPassword("admin"), AppUserId = user.UserId };
                            userRole = UserRoles.Admin;
                        }
                        else if (user.Email == "bojan.vasilic95@gmail.com")
                        {
                            //var _appUser = context.AppUsers.FirstOrDefault(a => a.FullName == "Manager");
                            identityUser = new RAIdentityUser() { Id = Guid.NewGuid().ToString(), UserName = "manager", Email = user.Email, PasswordHash = RAIdentityUser.HashPassword("manager"), AppUserId = user.UserId };
                            userRole = UserRoles.Manager;
                        }
                        else if (user.Email == "user@yahoo.com")
                        {
                            //var _appUser = context.AppUsers.FirstOrDefault(a => a.FullName == "AppUser AppUserovic");
                            identityUser = new RAIdentityUser() { Id = Guid.NewGuid().ToString(), UserName = "appu", Email = user.Email, PasswordHash = RAIdentityUser.HashPassword("appu"), AppUserId = user.UserId };
                            userRole = UserRoles.User;
                        }

                        var result = await userManager.CreateAsync(identityUser);

                        await userManager.AddToRolesAsync(identityUser, new List<string>() { userRole });
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}