# HƯỚNG DẪN HOÀN THIỆN DỰ ÁN PCM PRO EDITION

## 📋 CHECKLIST HIỆN TẠI

### ✅ ĐÃ HOÀN THÀNH

#### Domain Layer (100%)
- ✅ Tất cả Entities (Member, Booking, Tournament, Match, Court, etc.)
- ✅ Tất cả Enums (BookingStatus, TournamentType, MatchFormat, etc.)
- ✅ Domain Interfaces (IRepository, IUnitOfWork, ICacheService)

#### Application Layer (100%)
- ✅ Tất cả DTOs (Auth, Members, Wallet, Bookings, Tournaments, Matches)
- ✅ Service Interfaces (IAuthService, IMemberService, IWalletService, IBookingService, ITournamentService, IMatchService, IEloRatingService)

#### Infrastructure Layer (60%)
- ✅ PCMDbContext
- ✅ EF Configurations (Member, Booking, WalletTransaction)
- ✅ Repository & UnitOfWork implementations
- ✅ RedisCacheService
- ✅ EloRatingService
- ✅ WalletService (hoàn chỉnh)
- ✅ BookingService (hoàn chỉnh với Concurrency Control)
- ⏳ MemberService (cần implement)
- ⏳ AuthService (cần implement)
- ⏳ TournamentService (cần implement)
- ⏳ MatchService (cần implement)

#### API Layer (0%)
- ⏳ Controllers
- ⏳ SignalR Hubs
- ⏳ Middleware
- ⏳ Program.cs setup

#### Frontend (0%)
- ⏳ Tất cả

#### DevOps (0%)
- ⏳ Docker
- ⏳ CI/CD

---

## 🔧 BƯỚC TIẾP THEO - ƯU TIÊN

### BƯỚC 1: Hoàn thiện Infrastructure Services (Còn thiếu)

#### 1.1 MemberService

```csharp
// File: backend/Infrastructure/Services/MemberService.cs

using Microsoft.EntityFrameworkCore;
using PCM.Application.DTOs.Members;
using PCM.Application.Interfaces;
using PCM.Domain.Interfaces;
using PCM.Domain.Entities;

namespace PCM.Infrastructure.Services
{
    public class MemberService : IMemberService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;

        public MemberService(IUnitOfWork unitOfWork, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
        }

        public async Task<MemberDto?> GetByIdAsync(int id)
        {
            var member = await _unitOfWork.Members.GetByIdAsync(id);
            return member == null ? null : MapToDto(member);
        }

        public async Task<MemberDto?> GetByUserIdAsync(string userId)
        {
            var member = await _unitOfWork.Members.FirstOrDefaultAsync(m => m.UserId == userId);
            return member == null ? null : MapToDto(member);
        }

        public async Task<MemberDto?> UpdateProfileAsync(string userId, UpdateMemberProfileDto dto)
        {
            var member = await _unitOfWork.Members.FirstOrDefaultAsync(m => m.UserId == userId);
            if (member == null)
                throw new Exception("Member not found");

            if (!string.IsNullOrEmpty(dto.FullName))
                member.FullName = dto.FullName;

            if (!string.IsNullOrEmpty(dto.PhoneNumber))
                member.PhoneNumber = dto.PhoneNumber;

            if (dto.DateOfBirth.HasValue)
                member.DateOfBirth = dto.DateOfBirth;

            if (!string.IsNullOrEmpty(dto.AvatarUrl))
                member.AvatarUrl = dto.AvatarUrl;

            _unitOfWork.Members.Update(member);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(member);
        }

        public async Task<List<MemberDto>> GetTopRankingAsync(int count = 5)
        {
            // Try cache first
            var cacheKey = $"leaderboard:top:{count}";
            var cached = await _cacheService.GetAsync<List<MemberDto>>(cacheKey);
            if (cached != null)
                return cached;

            // Get from database
            var members = await _unitOfWork.Members.GetAllAsync();
            var top = members
                .Where(m => m.IsActive)
                .OrderByDescending(m => m.RankELO)
                .Take(count)
                .Select(MapToDto)
                .ToList();

            // Cache for 5 minutes
            await _cacheService.SetAsync(cacheKey, top, TimeSpan.FromMinutes(5));

            // Also update Redis sorted set for real-time leaderboard
            foreach (var member in members.Where(m => m.IsActive))
            {
                await _cacheService.AddToSortedSetAsync("leaderboard:elo", member.FullName, member.RankELO);
            }

            return top;
        }

        public async Task<bool> UpdateRankELOAsync(int memberId, double newELO)
        {
            var member = await _unitOfWork.Members.GetByIdAsync(memberId);
            if (member == null)
                return false;

            member.RankELO = newELO;
            _unitOfWork.Members.Update(member);
            await _unitOfWork.SaveChangesAsync();

            // Update cache
            await _cacheService.RemoveAsync($"leaderboard:top:5");
            await _cacheService.AddToSortedSetAsync("leaderboard:elo", member.FullName, newELO);

            return true;
        }

        private MemberDto MapToDto(Member member)
        {
            return new MemberDto
            {
                Id = member.Id,
                UserId = member.UserId,
                FullName = member.FullName,
                Email = member.Email,
                PhoneNumber = member.PhoneNumber,
                DateOfBirth = member.DateOfBirth,
                JoinDate = member.JoinDate,
                RankELO = member.RankELO,
                WalletBalance = member.WalletBalance,
                AvatarUrl = member.AvatarUrl,
                IsActive = member.IsActive
            };
        }
    }
}
```

#### 1.2 AuthService (JWT Authentication)

```csharp
// File: backend/Infrastructure/Services/AuthService.cs

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PCM.Application.DTOs.Auth;
using PCM.Application.Interfaces;
using PCM.Domain.Entities;
using PCM.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PCM.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<IdentityUser> userManager,
            IUnitOfWork unitOfWork,
            IConfiguration _configuration)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                throw new Exception("Invalid credentials");

            var validPassword = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!validPassword)
                throw new Exception("Invalid credentials");

            return await GenerateTokensAsync(user);
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new Exception("Email already registered");

            var user = new IdentityUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            // Create Member profile
            var member = new Member
            {
                UserId = user.Id,
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                DateOfBirth = dto.DateOfBirth,
                JoinDate = DateTime.UtcNow,
                RankELO = dto.InitialRankELO,
                WalletBalance = 0,
                IsActive = true
            };

            await _unitOfWork.Members.AddAsync(member);
            await _unitOfWork.SaveChangesAsync();

            // Assign default role
            await _userManager.AddToRoleAsync(user, "Member");

            return await GenerateTokensAsync(user);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
        {
            var principal = GetPrincipalFromExpiredToken(dto.AccessToken);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                throw new Exception("Invalid token");

            var refreshToken = await _unitOfWork.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken && rt.UserId == userId);

            if (refreshToken == null || refreshToken.IsUsed || refreshToken.IsRevoked || refreshToken.ExpiryDate < DateTime.UtcNow)
                throw new Exception("Invalid refresh token");

            // Mark as used
            refreshToken.IsUsed = true;
            _unitOfWork.RefreshTokens.Update(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            return await GenerateTokensAsync(user);
        }

        public async Task RevokeTokenAsync(string userId)
        {
            var tokens = await _unitOfWork.RefreshTokens.FindAsync(rt => rt.UserId == userId && !rt.IsRevoked);
            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                _unitOfWork.RefreshTokens.Update(token);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<AuthResponseDto> GenerateTokensAsync(IdentityUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secret = jwtSettings["Secret"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var accessTokenExpiration = int.Parse(jwtSettings["AccessTokenExpiration"]);
            var refreshTokenExpiration = int.Parse(jwtSettings["RefreshTokenExpiration"]);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpiration);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            // Create refresh token
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                JwtId = token.Id,
                IsUsed = false,
                IsRevoked = false,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMinutes(refreshTokenExpiration)
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = expiresAt
            };
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secret = jwtSettings["Secret"];

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
}
```

### BƯỚC 2: Tạo API Controllers

#### 2.1 AuthController

```csharp
// File: backend/API/Controllers/AuthController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PCM.Application.DTOs.Auth;
using PCM.Application.Interfaces;

namespace PCM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var result = await _authService.LoginAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var result = await _authService.RegisterAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return Unauthorized();

                await _authService.RevokeTokenAsync(userId);
                return Ok(new { message = "Token revoked successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
```

#### 2.2 MembersController

```csharp
// File: backend/API/Controllers/MembersController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PCM.Application.DTOs.Members;
using PCM.Application.Interfaces;

namespace PCM.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MembersController : ControllerBase
    {
        private readonly IMemberService _memberService;

        public MembersController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return Unauthorized();

                var member = await _memberService.GetByUserIdAsync(userId);
                if (member == null)
                    return NotFound();

                return Ok(member);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateMemberProfileDto dto)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return Unauthorized();

                var member = await _memberService.UpdateProfileAsync(userId, dto);
                return Ok(member);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("top-ranking")]
        public async Task<IActionResult> GetTopRanking([FromQuery] int count = 5)
        {
            try
            {
                var members = await _memberService.GetTopRankingAsync(count);
                return Ok(members);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
```

#### 2.3 BookingsController

```csharp
// File: backend/API/Controllers/BookingsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PCM.Application.DTOs.Bookings;
using PCM.Application.Interfaces;

namespace PCM.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("available-slots")]
        public async Task<IActionResult> GetAvailableSlots([FromQuery] DateTime date, [FromQuery] int? courtId = null)
        {
            try
            {
                var slots = await _bookingService.GetAvailableSlotsAsync(date, courtId);
                return Ok(slots);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return Unauthorized();

                var booking = await _bookingService.CreateBookingAsync(userId, dto);
                return Ok(booking);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("recurring")]
        public async Task<IActionResult> CreateRecurringBooking([FromBody] CreateRecurringBookingDto dto)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return Unauthorized();

                var result = await _bookingService.CreateRecurringBookingAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return Unauthorized();

                var result = await _bookingService.CancelBookingAsync(id, userId);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return Unauthorized();

                var bookings = await _bookingService.GetMyBookingsAsync(userId);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
```

### BƯỚC 3: Program.cs Setup

```csharp
// File: backend/API/Program.cs

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PCM.Application.Interfaces;
using PCM.Domain.Interfaces;
using PCM.Infrastructure.Persistence;
using PCM.Infrastructure.Persistence.Repositories;
using PCM.Infrastructure.Services;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PCM API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database
builder.Services.AddDbContext<PCMDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<PCMDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"])),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetValue<string>("Redis:ConnectionString");
    return ConnectionMultiplexer.Connect(configuration);
});

// SignalR
builder.Services.AddSignalR();

// Dependency Injection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IEloRatingService, EloRatingService>();
// Add more services...

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## 📝 NOTES

1. **Database Migration**: Sau khi tạo xong các EF Configurations, chạy:
   ```bash
   dotnet ef migrations add InitialCreate --project Infrastructure --startup-project API
   dotnet ef database update --project Infrastructure --startup-project API
   ```

2. **Seed Data**: Tạo seed data cho Categories, Courts, Admin user

3. **Frontend**: Sử dụng Vue 3 + Vite + Pinia + Vue Router + Axios

4. **Docker**: Tạo docker-compose.yml với SQL Server, Redis, Backend API, Frontend

5. **Testing**: Viết unit tests cho các services quan trọng

---

**Chúc bạn thành công!**
