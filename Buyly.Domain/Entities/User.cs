using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Buyly.Domain.Entities
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;

        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? PasswordResetCodeHash { get; set; }
        public DateTime? PasswordResetCodeExpiresAt { get; set; }
        public DateTime? PasswordResetCodeLastSentAt { get; set; }
        public DateTime? PasswordResetCodeRequestWindowStart { get; set; }
        public int PasswordResetCodeRequestCount { get; set; }
    }
}