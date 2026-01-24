# ⚡ QUICK START GUIDE

## 🎯 BẮT ĐẦU NGAY (5 PHÚT)

### Bước 1: Chuẩn bị môi trường

```bash
# Kiểm tra các công cụ cần thiết:
dotnet --version    # Cần .NET 8.0+
node --version      # Cần Node.js 18+
docker --version    # Cần Docker Desktop
```

### Bước 2: Thay thế Student ID

**QUAN TRỌNG:** Tìm và thay thế `XXX` bằng 3 số cuối MSSV của bạn trong các file sau:

```
backend/Infrastructure/Persistence/Configurations/
  ├── MemberConfiguration.cs         -> "XXX_Members"
  ├── BookingConfiguration.cs        -> "XXX_Bookings"
  ├── WalletTransactionConfiguration.cs -> "XXX_WalletTransactions"
  ├── TransactionCategoryConfiguration.cs -> "XXX_TransactionCategories"
  └── TreasuryTransactionConfiguration.cs -> "XXX_TreasuryTransactions"
```

**VD:** Nếu MSSV là 2054123 thì thay `XXX_Members` → `123_Members`

### Bước 3: Tạo .csproj files

Tạo các file project sau:

#### backend/Domain/Domain.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>PCM.Domain</RootNamespace>
  </PropertyGroup>
</Project>
```

#### backend/Application/Application.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>PCM.Application</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Domain\Domain.csproj" />
  </ItemGroup>
</Project>
```

#### backend/Infrastructure/Infrastructure.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>PCM.Infrastructure</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
    <PackageReference Include="StackExchange.Redis" Version="2.7.10" />
    <PackageReference Include="Hangfire" Version="1.8.6" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Application\Application.csproj" />
    <ProjectReference Include="..\Domain\Domain.csproj" />
  </ItemGroup>
</Project>
```

#### backend/API/API.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>PCM.API</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.SignalR" Version="1.1.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Infrastructure\Infrastructure.csproj" />
  </ItemGroup>
</Project>
```

### Bước 4: Tạo Program.cs

Copy code từ `IMPLEMENTATION_GUIDE.md` phần "Program.cs Setup" và tạo file:
```
backend/API/Program.cs
```

### Bước 5: Tạo missing services

Copy code từ `IMPLEMENTATION_GUIDE.md`:

1. **MemberService**
   - Tạo file: `backend/Infrastructure/Services/MemberService.cs`
   - Copy code từ section "1.1 MemberService"

2. **AuthService**
   - Tạo file: `backend/Infrastructure/Services/AuthService.cs`
   - Copy code từ section "1.2 AuthService"

### Bước 6: Tạo Controllers

Copy code từ `IMPLEMENTATION_GUIDE.md`:

1. **AuthController** → `backend/API/Controllers/AuthController.cs`
2. **MembersController** → `backend/API/Controllers/MembersController.cs`
3. **BookingsController** → `backend/API/Controllers/BookingsController.cs`

### Bước 7: Chạy migrations

```bash
cd backend

# Restore packages
dotnet restore

# Create migration
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project API

# Update database
dotnet ef database update --project Infrastructure --startup-project API
```

### Bước 8: Seed data cơ bản

Tạo file `backend/API/Data/SeedData.cs`:

```csharp
using Microsoft.AspNetCore.Identity;
using PCM.Domain.Entities;
using PCM.Domain.Enums;
using PCM.Infrastructure.Persistence;

namespace PCM.API.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<PCMDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Create roles
            string[] roleNames = { "Admin", "Member" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create admin user
            var adminEmail = "admin@pcm.vn";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(adminUser, "Admin@123");
                await userManager.AddToRoleAsync(adminUser, "Admin");

                // Create admin member profile
                var adminMember = new Member
                {
                    UserId = adminUser.Id,
                    FullName = "Administrator",
                    Email = adminEmail,
                    PhoneNumber = "0123456789",
                    JoinDate = DateTime.UtcNow,
                    RankELO = 1500,
                    WalletBalance = 0,
                    IsActive = true
                };
                context.Members.Add(adminMember);
            }

            // Create transaction categories
            if (!context.TransactionCategories.Any())
            {
                var categories = new List<TransactionCategory>
                {
                    new() { Name = "Nạp tiền", Type = TransactionType.Income, Scope = TransactionScope.Wallet },
                    new() { Name = "Phí sân", Type = TransactionType.Expense, Scope = TransactionScope.Wallet },
                    new() { Name = "Phí giải đấu", Type = TransactionType.Expense, Scope = TransactionScope.Wallet },
                    new() { Name = "Thưởng giải", Type = TransactionType.Income, Scope = TransactionScope.Wallet },
                    new() { Name = "Hoàn tiền", Type = TransactionType.Income, Scope = TransactionScope.Wallet },
                    new() { Name = "Thu phí thành viên", Type = TransactionType.Income, Scope = TransactionScope.Treasury },
                    new() { Name = "Chi phí vận hành", Type = TransactionType.Expense, Scope = TransactionScope.Treasury }
                };
                context.TransactionCategories.AddRange(categories);
            }

            // Create courts
            if (!context.Courts.Any())
            {
                var courts = new List<Court>
                {
                    new() { Name = "Sân 1", Description = "Sân thi đấu chính", HourlyRate = 100000, IsActive = true },
                    new() { Name = "Sân 2", Description = "Sân tập luyện", HourlyRate = 80000, IsActive = true },
                    new() { Name = "Sân 3", Description = "Sân VIP", HourlyRate = 150000, IsActive = true }
                };
                context.Courts.AddRange(courts);
            }

            await context.SaveChangesAsync();
        }
    }
}
```

Thêm vào Program.cs (trước `app.Run()`):

```csharp
// Seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}
```

### Bước 9: Chạy Backend

```bash
cd backend
dotnet run --project API

# Backend sẽ chạy tại: http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

### Bước 10: Chạy Frontend

```bash
cd frontend

# Install dependencies
npm install

# Run dev server
npm run dev

# Frontend sẽ chạy tại: http://localhost:3000
```

---

## 🧪 TEST ỨNG DỤNG

### 1. Test Authentication

**Đăng ký user mới:**
- Vào http://localhost:3000/register
- Điền form và đăng ký

**Đăng nhập:**
- Email: `admin@pcm.vn`
- Password: `Admin@123`

### 2. Test API qua Swagger

Vào http://localhost:5000/swagger

**Test flow:**
1. POST `/api/auth/login` → Lấy token
2. Click "Authorize" → Paste token
3. GET `/api/members/me` → Xem profile
4. GET `/api/members/top-ranking` → Xem BXH

### 3. Test Booking

1. GET `/api/bookings/available-slots?date=2026-01-25`
2. POST `/api/bookings` với body:
```json
{
  "courtId": 1,
  "startTime": "2026-01-25T10:00:00",
  "endTime": "2026-01-25T11:00:00",
  "note": "Test booking"
}
```

---

## 🐛 TROUBLESHOOTING

### Lỗi migration
```bash
# Xóa migrations cũ
rm -rf backend/Infrastructure/Migrations

# Tạo lại
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project API
dotnet ef database update --project Infrastructure --startup-project API
```

### Lỗi connection string
- Kiểm tra SQL Server đang chạy
- Kiểm tra password trong `appsettings.json`
- Thử connection string: `Server=localhost;Database=PCM_Advanced;Integrated Security=true;TrustServerCertificate=true`

### Lỗi Redis
```bash
# Chạy Redis qua Docker
docker run -d -p 6379:6379 redis:7-alpine
```

### Frontend không connect API
- Kiểm tra `.env` hoặc `vite.config.js`
- Đảm bảo API đang chạy tại `http://localhost:5000`

---

## 📦 ALTERNATIVE: Chạy toàn bộ bằng Docker

```bash
# Chỉ cần 1 lệnh!
docker-compose up -d

# Chờ 1-2 phút cho services khởi động
# Sau đó:
# - Frontend: http://localhost:3000
# - Backend: http://localhost:5000
# - SQL Server: localhost:1433
# - Redis: localhost:6379
```

---

## 📚 TÀI LIỆU THAM KHẢO

- `PROJECT_SUMMARY.md` - Tổng kết những gì đã làm
- `IMPLEMENTATION_GUIDE.md` - Code mẫu chi tiết
- `README_FULL.md` - Tài liệu đầy đủ

---

**Chúc bạn thành công! Nếu gặp vấn đề gì, check lại các file hướng dẫn trên. 🚀**
