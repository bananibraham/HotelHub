using DataAccessLayer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace HotelHub.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Invoice> Invoices { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<RoomType> RoomTypes { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;
        

        protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    // Dynamic Configurations
    builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

    // Room Relationships & Constraints
    builder.Entity<Room>()
        .HasOne(r => r.RoomType)
        .WithMany(rt => rt.Rooms)
        .HasForeignKey(r => r.RoomTypeId)
        .OnDelete(DeleteBehavior.Restrict);
}
    }
}