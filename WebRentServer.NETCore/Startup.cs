using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebRentServer.NETCore.Models.Entities;
using WebRentServer.NETCore.Persistance;
using WebRentServer.NETCore.Persistance.UnitOfWork;

namespace WebRentServer.NETCore
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddScoped<IUnitOfWork, UnitOfWork>(); 
            services.AddDbContext<RVDBContext>(options => options.UseSqlServer(Configuration.GetConnectionString("ConnStr")));
            
            services.AddIdentity<RAIdentityUser, IdentityRole>().AddEntityFrameworkStores<RVDBContext>();

            services.AddHostedService<DbDataConfiguration>();
            //services.AddAuthentication(options =>
            //{
            //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            //}).AddJwtBearer(options =>
            //{
            //    options.SaveToken = true; options.RequireHttpsMetadata = false;
            //    options.TokenValidationParameters = new TokenValidationParameters()
            //    {
            //        ValidateIssuer = true,
            //        ValidateAudience = true,
            //        ValidIssuer = Configuration["Jwt:ValidIssuer"],
            //        ValidAudience = Configuration["Jwt:ValidAudience"],
            //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Secret"]))
            //    };
            //});
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            //app.UseAuthentication();
            //app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            //DbDataConfiguration.Initialize(app);
        }
    }
}