using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.Domain.Entities;

namespace Buyly.Application.Interfaces
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        Task<IEnumerable<Review>> GetProductReviewsAsync(Guid productId);
        Task<Review?> GetUserReviewForProductAsync(Guid productId, string userId);
        Task<bool> HasUserPurchasedProductAsync(Guid productId, string userId);
        Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid productId);

        Task<ReviewVote?> GetUserVoteForReviewAsync(Guid reviewId, string userId);
        Task AddVoteAsync(ReviewVote reviewVote);
        Task RemoveVoteAsync(ReviewVote reviewVote);
    }
}