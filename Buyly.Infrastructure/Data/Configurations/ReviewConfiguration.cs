using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Buyly.Infrastructure.Data.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("Reviews", tb =>
            {
                tb.HasCheckConstraint("CK_Reviews_Rating_Range", "`Rating` BETWEEN 1 AND 5");
            });

            builder.HasKey(r => r.Id);
            builder.Property(r => r.Rating)
                .IsRequired();

            builder.Property(r => r.Comment)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(r => r.Upvotes)
                .HasDefaultValue(0);

            builder.Property(r => r.Downvotes)
                .HasDefaultValue(0);

            builder.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(r => new { r.UserId, r.ProductId })
                .IsUnique();

            builder.HasIndex(r => r.ProductId);
            builder.HasIndex(r => r.UserId);
        }
    }
}