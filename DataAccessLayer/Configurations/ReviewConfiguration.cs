using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(r => r.ReviewId);
            builder.Property(r => r.Rating).IsRequired();
            builder.Property(r => r.Comment).HasMaxLength(1000);
            builder.HasQueryFilter(r => r.Customer == null || r.Customer.IsActive);

            builder.HasOne(r => r.Customer)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Booking)
                .WithMany()
                .HasForeignKey(r => r.BookingId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}