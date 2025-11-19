using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.Application.Interfaces;
using Buyly.Domain.Entities;
using Buyly.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Buyly.Infrastructure.Repositories
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        public ReviewRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Review>> GetProductReviewsAsync(Guid productId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToListAsync();

            var distribution = new Dictionary<int, int>
            {
                { 1, 0 },
                { 2, 0 },
                { 3, 0 },
                { 4, 0 },
                { 5, 0 }
            };

            foreach (var review in reviews)
            {
                distribution[review.Rating] = review.Count;
            }
            return distribution;
        }

        public async Task<Review?> GetUserReviewForProductAsync(Guid productId, string userId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId);
        }

        public async Task<bool> HasUserPurchasedProductAsync(Guid productId, string userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Delivered)
                .SelectMany(o => o.OrderItems)
                .AnyAsync(oi => oi.ProductId == productId);
        }

        public async Task<ReviewVote?> GetUserVoteForReviewAsync(Guid reviewId, string userId)
        {
            return await _context.ReviewVotes
                .FirstOrDefaultAsync(rv => rv.ReviewId == reviewId && rv.UserId == userId);
        }

        public async Task AddVoteAsync(ReviewVote reviewVote)
        {
            await _context.ReviewVotes.AddAsync(reviewVote);
        }

        public Task RemoveVoteAsync(ReviewVote reviewVote)
        {
            _context.ReviewVotes.Remove(reviewVote);
            return Task.CompletedTask;
        }
    }
}