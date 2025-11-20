using Buyly.API.Models;
using Buyly.Application.DTOs.Auth;
using Buyly.Application.Interfaces;
using Buyly.Application.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;

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
        [SwaggerOperation(
            Summary = "Register a new user",
            Description = "Creates a customer profile (names, email, phone, address), hashes the password through ASP.NET Identity, assigns the Customer role, and returns a JWT for immediate use."
        )]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var authResponse = await _authService.RegisterAsync(dto);
            return Success(authResponse, "User registered successfully.");
        }

        [EnableRateLimiting(RateLimitPolicies.Strict)]
        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Authenticate user",
            Description = "Validates credentials via Identity, then issues a JWT with role claims and profile data so the client can authorize subsequent calls."
        )]
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
        [SwaggerOperation(
            Summary = "Request password reset code",
            Description = "Sends a verification code by email (if the account exists) and enforces cooldown + per-window limits to prevent abuse."
        )]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            await _authService.ForgotPasswordAsync(dto);
            return Success("If an account with that email exists, a reset code has been sent.");
        }

        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicies.Strict)]
        [HttpPost("reset-password")]
        [SwaggerOperation(
            Summary = "Reset password",
            Description = "Validates the emailed verification code, resets the user’s password through ASP.NET Identity, and clears reset counters."
        )]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            await _authService.ResetPasswordAsync(dto);
            return Success("Password reset successfully.");
        }
    }
}
