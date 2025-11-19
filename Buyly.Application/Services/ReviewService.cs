using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.Application.DTOs.Review;
using Buyly.Application.Exceptions;
using Buyly.Application.Interfaces;
using Buyly.Domain.Entities;

namespace Buyly.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ReviewService(IReviewRepository reviewRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
        {
            _reviewRepository = reviewRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ReviewDto> CreateReviewAsync(string userId, CreateReviewDto createReviewDto)
        {
            var product = await _productRepository.GetByIdAsync(createReviewDto.ProductId);
            if (product == null)
            {
                throw new NotFoundException("Product", createReviewDto.ProductId.ToString());
            }

            var existingReview = await _reviewRepository.GetUserReviewForProductAsync(createReviewDto.ProductId, userId);
            if (existingReview != null)
            {
                throw new ValidationException("User has already reviewed this product.");
            }

            var hasPurchased = await _reviewRepository.HasUserPurchasedProductAsync(createReviewDto.ProductId, userId);

            var review = new Review
            {
                ProductId = createReviewDto.ProductId,
                UserId = userId,
                Rating = createReviewDto.Rating,
                Comment = createReviewDto.Comment,
                IsVerifiedPurchase = hasPurchased,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);
            await _unitOfWork.CommitAsync();

            await UpdateProductRatingAsync(createReviewDto.ProductId);

            await _unitOfWork.CommitAsync();

            review = await _reviewRepository.GetUserReviewForProductAsync(createReviewDto.ProductId, userId) ?? review;

            return MapToReviewDto(review);
        }

        public async Task<bool> DeleteReviewAsync(string userId, Guid reviewId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);

            if (review == null || review.UserId != userId)
            {
                return false;
            }

            var productId = review.ProductId;
            _reviewRepository.Delete(review);
            await _unitOfWork.CommitAsync();

            await UpdateProductRatingAsync(productId);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(Guid productId)
        {
            var reviews = await _reviewRepository.GetProductReviewsAsync(productId);
            return reviews.Select(MapToReviewDto);
        }

        public async Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(Guid productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new NotFoundException("Product", productId.ToString());
            }

            var distribution = await _reviewRepository.GetRatingDistributionAsync(productId);

            return new ProductReviewSummaryDto
            {
                AverageRating = product.AverageRating,
                TotalReviews = product.ReviewCount,
                FiveStarCount = distribution[5],
                FourStarCount = distribution[4],
                ThreeStarCount = distribution[3],
                TwoStarCount = distribution[2],
                OneStarCount = distribution[1]
            };
        }

        public async Task<ReviewDto?> GetUserReviewForProductAsync(string userId, Guid productId)
        {
            var review = await _reviewRepository.GetUserReviewForProductAsync(productId, userId);

            return review == null ? null : MapToReviewDto(review);
        }

        public async Task<ReviewDto> UpdateReviewAsync(string userId, Guid reviewId, UpdateReviewDto dto)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);

            if (review == null || review.UserId != userId)
            {
                throw new NotFoundException("Review", reviewId.ToString());
            }

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.UpdatedAt = DateTime.UtcNow;

            _reviewRepository.Update(review);
            await _unitOfWork.CommitAsync();

            await UpdateProductRatingAsync(review.ProductId);
            await _unitOfWork.CommitAsync();

            review = await _reviewRepository.GetByIdAsync(reviewId) ?? review;
            review = await _reviewRepository.GetUserReviewForProductAsync(review.ProductId, userId) ?? review;

            return MapToReviewDto(review);
        }

        public async Task<bool> VoteReviewAsync(string userId, Guid reviewId, bool isUpvote)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
            {
                return false;
            }

            var existingVote = await _reviewRepository.GetUserVoteForReviewAsync(reviewId, userId);

            if (existingVote != null)
            {
                if (existingVote.IsUpvote != isUpvote)
                {
                    if (existingVote.IsUpvote) review.Upvotes--;
                    else review.Downvotes--;

                    if (isUpvote) review.Upvotes++;
                    else review.Downvotes++;

                    existingVote.IsUpvote = isUpvote;
                }
                else
                {
                    if (existingVote.IsUpvote) review.Upvotes--;
                    else review.Downvotes--;

                    await _reviewRepository.RemoveVoteAsync(existingVote);
                }
            }
            else
            {
                var vote = new ReviewVote
                {
                    ReviewId = reviewId,
                    UserId = userId,
                    IsUpvote = isUpvote
                };

                await _reviewRepository.AddVoteAsync(vote);

                if (isUpvote) review.Upvotes++;
                else review.Downvotes++;
            }

            _reviewRepository.Update(review);
            await _unitOfWork.CommitAsync();

            return true;
        }

        private async Task UpdateProductRatingAsync(Guid productId)
        {
            var reviews = await _reviewRepository.GetProductReviewsAsync(productId);
            var reviewsList = reviews.ToList();

            var product = await _productRepository.GetByIdAsync(productId);

            if (product != null)
            {
                product.ReviewCount = reviewsList.Count;
                product.AverageRating = reviewsList.Any()
                    ? Math.Round(reviewsList.Average(r => r.Rating), 2)
                    : 0;

                _productRepository.Update(product);
            }
        }

        private static ReviewDto MapToReviewDto(Review review)
        {
            return new ReviewDto
            {
                Id = review.Id,
                ProductId = review.ProductId,
                ProductName = review.Product?.Name ?? "",
                UserId = review.UserId,
                UserName = $"{review.User?.FirstName} {review.User?.LastName}",
                Rating = review.Rating,
                Comment = review.Comment,
                IsVerifiedPurchase = review.IsVerifiedPurchase,
                CreatedAt = review.CreatedAt,
                Upvotes = review.Upvotes,
                Downvotes = review.Downvotes
            };
        }
    }
}