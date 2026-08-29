using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.HasKey(i => i.InvoiceId);

            builder.Property(i => i.InvoiceNumber)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(i => i.TotalAmount)
                   .HasPrecision(18, 2);

            builder.Property(i => i.PaidAmount)
                   .HasPrecision(18, 2);

            builder.Property(i => i.RemainingAmount)
                   .HasPrecision(18, 2);

            builder.HasIndex(i => i.BookingId)
                   .IsUnique();

            builder.HasOne(i => i.Booking)
                   .WithOne()
                   .HasForeignKey<Invoice>(i => i.BookingId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
