using WebRentServer.NETCore.Models.Entities;
using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace WebRentServer.NETCore.Persistance
{
    public class RVDBContext : IdentityDbContext<RAIdentityUser>
    {
        public DbSet<Service> Services { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Office> Offices { get; set; }
        public DbSet<OfficePicture> OfficePictures { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<RentService> RentServices { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehiclePicture> VehiclePictures { get; set; }
        public DbSet<TypeOfVehicle> TypesOfVehicles { get; set; }

        public RVDBContext() : base() { }
        public RVDBContext(DbContextOptions<RVDBContext> options) : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=RentVDB;Integrated Security=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Order>()
                .HasOne(e => e.ReturnOffice)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Order>()
                .HasOne(e => e.DepartureOffice)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Order>()
                .HasOne(e => e.User)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}