using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PCM.Application.DTOs.Auth;
using PCM.Application.Interfaces;
using PCM.Domain.Entities;
using PCM.Domain.Interfaces;

namespace PCM.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;

        public AuthService(UserManager<IdentityUser> userManager, IUnitOfWork unitOfWork, IConfiguration config)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _config = config;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) throw new Exception("Invalid credentials");
            var ok = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!ok) throw new Exception("Invalid credentials");
            return await GenerateTokensAsync(user);
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var user = new IdentityUser { UserName = dto.Email, Email = dto.Email };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) throw new Exception(string.Join(';', result.Errors.Select(e => e.Description)));

            // create domain Member
            var member = new Member
            {
                UserId = user.Id,
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                DateOfBirth = dto.DateOfBirth,
                RankELO = dto.InitialRankELO,
                JoinDate = DateTime.UtcNow
            };
            await _unitOfWork.Members.AddAsync(member);
            await _unitOfWork.SaveChangesAsync();

            await _userManager.AddToRoleAsync(user, "Member");

            return await GenerateTokensAsync(user);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
        {
            // Basic refresh logic: find stored refresh token and validate expiry
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(dto.AccessToken);
            var jti = jwt.Id ?? string.Empty;

            var stored = (await _unitOfWork.RefreshTokens.FindAsync(rt => rt.Token == dto.RefreshToken)).FirstOrDefault();
            if (stored == null || stored.IsRevoked || stored.IsUsed || stored.ExpiryDate < DateTime.UtcNow)
                throw new Exception("Invalid refresh token");

            // mark used
            stored.IsUsed = true;
            await _unitOfWork.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(stored.UserId);
            if (user == null) throw new Exception("User not found");

            return await GenerateTokensAsync(user);
        }

        public async Task RevokeTokenAsync(string userId)
        {
            var tokens = (await _unitOfWork.RefreshTokens.FindAsync(rt => rt.UserId == userId && !rt.IsRevoked));
            foreach (var t in tokens) t.IsRevoked = true;
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<AuthResponseDto> GenerateTokensAsync(IdentityUser user)
        {
            var secret = _config["Jwt:Secret"]!;
            var issuer = _config["Jwt:Issuer"]!;
            var audience = _config["Jwt:Audience"]!;
            var minutes = int.Parse(_config["Jwt:ExpirationMinutes"] ?? "60");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(minutes),
                signingCredentials: creds
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refresh = new RefreshToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                JwtId = token.Id ?? string.Empty,
                IsUsed = false,
                IsRevoked = false,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            await _unitOfWork.RefreshTokens.AddAsync(refresh);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refresh.Token,
                ExpiresAt = token.ValidTo
            };
        }
    }
}
