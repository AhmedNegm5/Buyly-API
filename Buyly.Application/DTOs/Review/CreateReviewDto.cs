using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Buyly.Application.DTOs.Review
{
    public class CreateReviewDto
    {
        [Required]
        public Guid ProductId { get; set; }
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [Required]
        [MinLength(10)]
        [MaxLength(1000)]
        public string Comment { get; set; } = null!;
    }
}