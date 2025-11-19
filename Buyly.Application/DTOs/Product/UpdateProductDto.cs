using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Buyly.Application.DTOs.Product
{
    public class UpdateProductDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal? Price { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int? Stock { get; set; }
        public Guid? CategoryId { get; set; }
        public string? ImageUrl { get; set; }
    }
}