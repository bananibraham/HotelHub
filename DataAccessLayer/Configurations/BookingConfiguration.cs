using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.BookingId);
            builder.Property(b => b.Status).IsRequired().HasMaxLength(50);
            builder.Property(b => b.CheckInDate).IsRequired();
            builder.Property(b => b.CheckOutDate).IsRequired();
            builder.Property(b => b.TotalPrice).HasColumnType("decimal(18,2)");
            builder.HasQueryFilter(b => b.IsActive);

            builder.HasOne(b => b.Customer)
                   .WithMany(c => c.Bookings)
                   .HasForeignKey(b => b.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Room)
                   .WithMany(r => r.Bookings)
                   .HasForeignKey(b => b.RoomId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
