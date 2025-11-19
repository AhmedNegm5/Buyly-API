using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buyly.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }

        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public string? ImageUrl { get; set; }

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public double AverageRating { get; set; } = 0.0;
        public int ReviewCount { get; set; }
    }
}