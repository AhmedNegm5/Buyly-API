using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Buyly.Infrastructure.Data.Configurations
{
    public class ReviewVoteConfiguration : IEntityTypeConfiguration<ReviewVote>
    {
        public void Configure(EntityTypeBuilder<ReviewVote> builder)
        {
            builder.ToTable("ReviewVotes");

            builder.HasKey(rv => rv.Id);

            builder.Property(rv => rv.IsUpvote)
                   .IsRequired();

            builder.HasOne(rv => rv.Review)
                    .WithMany(r => r.Votes)
                    .HasForeignKey(rv => rv.ReviewId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rv => rv.User)
                    .WithMany()
                    .HasForeignKey(rv => rv.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(rv => new { rv.ReviewId, rv.UserId })
                   .IsUnique();

        }
    }
}