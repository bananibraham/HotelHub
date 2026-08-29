using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.PaymentId);

            builder.Property(p => p.Amount)
                   .HasPrecision(18, 2);

            builder.Property(p => p.PaymentMethod)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(p => p.TransactionId)
                   .HasMaxLength(100);

            builder.Property(p => p.Notes)
                   .HasMaxLength(500);

            builder.HasOne(p => p.Booking)
                   .WithMany()
                   .HasForeignKey(p => p.BookingId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
