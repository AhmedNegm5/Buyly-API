using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.Application.DTOs.Review;

namespace Buyly.Application.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewDto> CreateReviewAsync(string userId, CreateReviewDto createReviewDto);
        Task<ReviewDto> UpdateReviewAsync(string userId, Guid reviewId, UpdateReviewDto dto);
        Task<bool> DeleteReviewAsync(string userId, Guid reviewId);
        Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(Guid productId);
        Task<ReviewDto?> GetUserReviewForProductAsync(string userId, Guid productId);
        Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(Guid productId);
        Task<bool> VoteReviewAsync(string userId, Guid reviewId, bool isUpvote);
    }
}