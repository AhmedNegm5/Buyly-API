using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buyly.Application.DTOs.Cart
{
    public class CartDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public decimal TotalItems { get; set; }
        public decimal Total { get; set; }
    }
}