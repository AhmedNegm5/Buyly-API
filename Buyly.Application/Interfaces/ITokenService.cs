using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.Domain.Entities;

namespace Buyly.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user, IList<string> roles);
    }
}