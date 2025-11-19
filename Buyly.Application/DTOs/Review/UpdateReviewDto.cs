using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Buyly.Application.DTOs.Review
{
    public class UpdateReviewDto
    {
        [Required]
        public int Rating { get; set; }
        [Required]
        [MinLength(10)]
        [MaxLength(1000)]
        public string Comment { get; set; } = null!;
    }
}