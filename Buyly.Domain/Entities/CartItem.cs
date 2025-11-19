using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buyly.Domain.Entities
{
    public class CartItem : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;

        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal PriceAtAdd { get; set; }
    }
}