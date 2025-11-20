using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Buyly.Application.DTOs.Review;
using Buyly.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Buyly.API.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Buyly.API.Controllers
{
    public class ReviewsController : BaseApiController
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost]
        [Authorize]
        [SwaggerOperation(
            Summary = "Create a review",
            Description = "Allows a signed-in customer to leave a rating/comment for a product they purchased."
        )]
        [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto createReviewDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? throw new UnauthorizedAccessException("User ID not found in token.");

            var review = await _reviewService.CreateReviewAsync(userId, createReviewDto);

            return Created(review, "Review created successfully.");
        }

        [HttpGet("product/{productId:guid}")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "List product reviews",
            Description = "Returns public reviews for a specific product, sorted by recency."
        )]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ReviewDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductReviews([FromRoute] Guid productId)
        {
            var reviews = await _reviewService.GetProductReviewsAsync(productId);
            return Success(reviews, "Product reviews retrieved successfully.");
        }

        [HttpGet("product/{productId:guid}/summary")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Get product review summary",
            Description = "Returns aggregate rating counts and averages for a product."
        )]
        [ProducesResponseType(typeof(ApiResponse<ProductReviewSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductReviewSummary([FromRoute] Guid productId)
        {
            var summary = await _reviewService.GetProductReviewSummaryAsync(productId);
            return Success(summary, "Product review summary retrieved successfully.");
        }

        [HttpGet("my-review/product/{productId:guid}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Get my review for a product",
            Description = "Returns the authenticated user’s personal review for a product, if it exists."
        )]
        [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyReviewForProduct([FromRoute] Guid productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? throw new UnauthorizedAccessException("User ID not found in token.");

            var review = await _reviewService.GetUserReviewForProductAsync(userId, productId);

            if (review == null)
            {
                return NotFound("Review not found for the specified product.");
            }

            return Success(review, "User review for product retrieved successfully.");
        }

        [HttpPut("{reviewId:guid}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Update a review",
            Description = "Allows the review author to modify rating or comment text."
        )]
        [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateReview([FromRoute] Guid reviewId, [FromBody] UpdateReviewDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? throw new UnauthorizedAccessException("User ID not found in token.");

            var updatedReview = await _reviewService.UpdateReviewAsync(userId, reviewId, dto);

            return Success(updatedReview, "Review updated successfully.");
        }

        [HttpDelete("{reviewId:guid}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Delete a review",
            Description = "Soft-deletes a review if it belongs to the authenticated user."
        )]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteReview([FromRoute] Guid reviewId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? throw new UnauthorizedAccessException("User ID not found in token.");

            var result = await _reviewService.DeleteReviewAsync(userId, reviewId);

            if (!result)
            {
                return NotFound("Review not found or you do not have permission to delete it.");
            }

            return Success("Review deleted successfully.");
        }

        [HttpPost("{reviewId:guid}/vote")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Vote on a review",
            Description = "Marks a review as helpful/unhelpful (upvote/downvote) for the current user."
        )]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VoteReview([FromRoute] Guid reviewId, [FromQuery] bool isUpvote)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? throw new UnauthorizedAccessException("User ID not found in token.");

            var result = await _reviewService.VoteReviewAsync(userId, reviewId, isUpvote);

            if (!result)
            {
                return NotFound("Review not found.");
            }

            return Success("Vote recorded successfully.");
        }

    }
}
