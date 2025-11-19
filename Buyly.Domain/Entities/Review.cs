using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buyly.Domain.Entities
{
    public class Review : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public Guid ProductId { get; set; }

        public User User { get; set; } = null!;
        public Product Product { get; set; } = null!;

        public int Rating { get; set; }
        public string Comment { get; set; } = null!;

        public bool IsVerifiedPurchase { get; set; }

        public int Upvotes { get; set; } = 0;
        public int Downvotes { get; set; } = 0;

        public ICollection<ReviewVote> Votes { get; set; } = new List<ReviewVote>();
    }
}