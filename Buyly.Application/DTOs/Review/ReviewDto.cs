using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buyly.Application.DTOs.Review
{
    public class ReviewDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public int Rating { get; set; }
        public string Comment { get; set; } = null!;
        public bool IsVerifiedPurchase { get; set; }
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}