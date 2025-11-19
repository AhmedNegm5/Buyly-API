using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.Application.Configurations;
using Buyly.Application.DTOs.Auth;
using Buyly.Application.Exceptions;
using Buyly.Application.Interfaces;
using Buyly.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Buyly.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly TimeSpan _resetCodeCooldown;
        private readonly TimeSpan _resetCodeWindow;
        private readonly int _resetCodeWindowLimit;

        public AuthService(
            ITokenService tokenService,
            UserManager<User> userManager,
            IEmailSender emailSender,
            IOptions<ResetPasswordSettings> resetPasswordOptions)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _emailSender = emailSender;
            var settings = resetPasswordOptions.Value ?? new ResetPasswordSettings();
            _resetCodeCooldown = TimeSpan.FromSeconds(Math.Max(0, settings.CooldownSeconds));
            _resetCodeWindow = TimeSpan.FromMinutes(Math.Max(0, settings.WindowMinutes));
            _resetCodeWindowLimit = settings.WindowLimit <= 0 ? 5 : settings.WindowLimit;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                throw new ValidationException("Invalid email or password.");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!isPasswordValid)
            {
                throw new ValidationException("Invalid email or password.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateToken(user, roles);

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = roles.FirstOrDefault() ?? "Customer"
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);

            if (existingUser != null)
            {
                throw new ValidationException("User with this email already exists.");
            }

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                UserName = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                throw new ValidationException(errors);
            }

            await _userManager.AddToRoleAsync(user, "Customer");

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateToken(user, roles);

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = "Customer"
            };
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                // Do not reveal if user exists
                return;
            }

            var now = DateTime.UtcNow;
            if (_resetCodeCooldown > TimeSpan.Zero &&
                user.PasswordResetCodeLastSentAt.HasValue &&
                now - user.PasswordResetCodeLastSentAt.Value < _resetCodeCooldown)
            {
                var waitSeconds = (_resetCodeCooldown - (now - user.PasswordResetCodeLastSentAt.Value)).TotalSeconds;
                throw new ValidationException($"Please wait {Math.Ceiling(waitSeconds)} seconds before requesting another reset code.");
            }

            if (_resetCodeWindow == TimeSpan.Zero ||
                !user.PasswordResetCodeRequestWindowStart.HasValue ||
                now - user.PasswordResetCodeRequestWindowStart.Value >= _resetCodeWindow)
            {
                user.PasswordResetCodeRequestWindowStart = now;
                user.PasswordResetCodeRequestCount = 0;
            }

            if (_resetCodeWindowLimit > 0 &&
                user.PasswordResetCodeRequestCount >= _resetCodeWindowLimit)
            {
                throw new ValidationException("You have reached the maximum number of reset code requests for the current window. Please try again later.");
            }

            var code = GenerateResetCode();
            user.PasswordResetCodeHash = HashResetCode(code);
            user.PasswordResetCodeExpiresAt = DateTime.UtcNow.AddMinutes(10);
            user.PasswordResetCodeLastSentAt = now;
            user.PasswordResetCodeRequestCount += 1;

            await _userManager.UpdateAsync(user);

            var body = $@"
                <p>Hello {WebUtility.HtmlEncode(user.FirstName)},</p>
                <p>You requested to reset your Buyly password. Use the verification code below within the next 10 minutes:</p>
                <p style=""font-size:24px;font-weight:bold;"">{code}</p>
                <p>If you did not request this, please ignore this email.</p>";
            await _emailSender.SendEmailAsync(dto.Email, "Reset your Buyly password", body);

        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                throw new NotFoundException("User", dto.Email);
            }

            if (user.PasswordResetCodeHash == null || user.PasswordResetCodeExpiresAt == null || user.PasswordResetCodeExpiresAt < DateTime.UtcNow)
            {
                throw new ValidationException("Reset code is invalid or has expired.");
            }

            if (!CodesMatch(dto.Code, user.PasswordResetCodeHash))
            {
                throw new ValidationException("Invalid reset code.");
            }
            if (dto.NewPassword != dto.ConfirmPassword)
            {
                throw new ValidationException("Passwords do not match.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                throw new ValidationException(errors);
            }

            user.PasswordResetCodeHash = null;
            user.PasswordResetCodeExpiresAt = null;
            user.PasswordResetCodeLastSentAt = null;
            user.PasswordResetCodeRequestWindowStart = null;
            user.PasswordResetCodeRequestCount = 0;
            await _userManager.UpdateAsync(user);
        }

        private static string GenerateResetCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
        }

        private static string HashResetCode(string code)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(code));
            return Convert.ToHexString(hashBytes);
        }

        private static bool CodesMatch(string code, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(storedHash))
            {
                return false;
            }

            var providedHash = HashResetCode(code);
            var providedBytes = Convert.FromHexString(providedHash);
            var storedBytes = Convert.FromHexString(storedHash);

            return CryptographicOperations.FixedTimeEquals(providedBytes, storedBytes);
        }
    }
}
