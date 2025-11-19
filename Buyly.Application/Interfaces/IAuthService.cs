using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.Application.DTOs.Auth;

namespace Buyly.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task ForgotPasswordAsync(ForgotPasswordDto dto);
        Task ResetPasswordAsync(ResetPasswordDto dto);
    }
}
