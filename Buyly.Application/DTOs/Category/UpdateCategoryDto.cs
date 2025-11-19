using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buyly.Application.DTOs.Category
{
    public class UpdateCategoryDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}