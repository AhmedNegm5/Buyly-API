using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Buyly.Application.DTOs.Auth
{
    public class RegisterDto
    {
        [Required]
        public string FirstName { get; set; } = null!;
        [Required]
        public string LastName { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;
        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }
}