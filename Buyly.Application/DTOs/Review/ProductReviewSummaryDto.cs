using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buyly.Application.DTOs.Review
{
    public class ProductReviewSummaryDto
    {
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
    }
}