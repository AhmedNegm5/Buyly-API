using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buyly.Domain.Entities
{
    public class ReviewVote : BaseEntity
    {
        public Guid ReviewId { get; set; }
        public Review Review { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;

        public bool IsUpvote { get; set; }
    }
}