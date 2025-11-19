using Buyly.API.Models;
using Buyly.Application.DTOs.Auth;
using Buyly.Application.Interfaces;
using Buyly.Application.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;

namespace Buyly.API.Controllers
{
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [EnableRateLimiting(RateLimitPolicies.Strict)]
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var authResponse = await _authService.RegisterAsync(dto);
            return Success(authResponse, "User registered successfully.");
        }

        [EnableRateLimiting(RateLimitPolicies.Strict)]
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var authResponse = await _authService.LoginAsync(dto);
            return Success(authResponse, "User logged in successfully.");
        }

        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicies.Strict)]
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            await _authService.ForgotPasswordAsync(dto);
            return Success("If an account with that email exists, a reset code has been sent.");
        }

        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicies.Strict)]
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            await _authService.ResetPasswordAsync(dto);
            return Success("Password reset successfully.");
        }
    }
}
