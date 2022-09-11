using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebRentServer.NETCore.Models.Entities;
using WebRentServer.NETCore.Persistance;
using WebRentServer.NETCore.Persistance.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebRentServer.NETCore.JwtHelpers;
using WebRentServer.NETCore.Encrypting;
using WebRentServer.NETCore.Models;

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
            services.AddOptions();
            if (SecretKey.StoreKey(SecretKey.GenerateKey(AlgorithmType.AES), typeof(AESConfig).Name))
                services.Configure<AESConfig>(Configuration.GetSection(typeof(AESConfig).Name));

            services.AddControllers();

            services.AddDbContext<RVDBContext>(options => options.UseSqlServer(Configuration.GetConnectionString("ConnStr")));
            services.AddTransient<IUnitOfWork, UnitOfWork>();

            services.AddIdentity<RAIdentityUser, IdentityRole>().AddEntityFrameworkStores<RVDBContext>();

            services.AddHostedService<DbDataConfiguration>();
            var bindJWTSettings = new JwtSettings();

            Configuration.Bind("JsonWebTokenKeys", bindJWTSettings);
            services.AddSingleton(bindJWTSettings);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = bindJWTSettings.ValidateIssuerSigningKey,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(bindJWTSettings.IssuerSigningKey)),
                    ValidateIssuer = bindJWTSettings.ValidateIssuer,
                    ValidateAudience = bindJWTSettings.ValidateAudience,
                    ValidIssuer = bindJWTSettings.ValidIssuer,
                    ValidAudience = bindJWTSettings.ValidAudience,
                    RequireExpirationTime = bindJWTSettings.RequireExpirationTime,
                    ValidateLifetime = bindJWTSettings.ValidateLifetime,
                    ClockSkew = TimeSpan.FromDays(1)
                };
            });

            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme."
                });
                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
                    {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer"
                    }
                },
                new string[] {}
                    }
                });
            });
            services.AddSwaggerGen();
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}