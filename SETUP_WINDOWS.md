# 🚀 HƯỚNG DẪN CHẠY DỰ ÁN PCM (Windows + SQL Server + Windows Auth)

Bạn đã hoàn tất việc thay MSSV = **020**. Dưới đây là từng bước để chạy dự án ngay!

---

## 1️⃣ CHUẨN BỊ MÔI TRƯỜNG

### ✅ Kiểm tra các công cụ cần thiết

```bash
# Kiểm tra .NET 8
dotnet --version

# Kiểm tra Node.js
node --version

# Kiểm tra SQL Server đã cài
sqlcmd -S (local) -U sa -P YourPassword
# Nếu dùng Windows Auth thì không cần -U và -P

# Kiểm tra Redis (nếu cài)
redis-cli --version
```

### ✅ Tạo Database (Windows Auth)

Mở **SQL Server Management Studio** hoặc **sqlcmd**:

```sql
-- Tạo database
CREATE DATABASE [020_PCM];
GO

-- Kiểm tra
SELECT name FROM sys.databases WHERE name = '020_PCM';
GO
```

---

## 2️⃣ BUILD & RUN BACKEND

### Bước 1: Mở Command Prompt tại thư mục Backend

```bash
cd backend/API
```

### Bước 2: Restore dependencies

```bash
dotnet restore
```

### Bước 3: Chạy Database Migrations

```bash
# Từ thư mục API
dotnet ef migrations add InitialCreate `
    --project ../Infrastructure/Infrastructure.csproj `
    --startup-project API.csproj

dotnet ef database update `
    --project ../Infrastructure/Infrastructure.csproj `
    --startup-project API.csproj
```

### Bước 4: Chạy Backend

```bash
dotnet run
```

**Lưu ý:** Bạn sẽ thấy thông báo:
- ✅ Database seeded successfully
- ✅ Admin user created (admin@pcm.vn / Admin@123)
- ✅ 3 sân được tạo (Sân 1, Sân 2, Sân 3)
- ✅ 7 transaction categories được tạo

**Backend chạy tại:** `http://localhost:5000`
**Swagger UI:** `http://localhost:5000/swagger`

---

## 3️⃣ SETUP & RUN FRONTEND

### Bước 1: Mở Command Prompt mới tại thư mục Frontend

```bash
cd frontend
```

### Bước 2: Cài dependencies

```bash
npm install
```

### Bước 3: Chạy development server

```bash
npm run dev
```

**Frontend chạy tại:** `http://localhost:3000`

---

## 4️⃣ TEST ỨNG DỤNG

### ✅ Test Swagger API

1. Vào `http://localhost:5000/swagger`
2. **POST** `/api/auth/login`
   ```json
   {
     "email": "admin@pcm.vn",
     "password": "Admin@123"
   }
   ```
3. Copy token từ response
4. Click **"Authorize"** → Paste token
5. Test các endpoint khác

### ✅ Test Frontend

1. Vào `http://localhost:3000`
2. Click **"Login"**
3. Đăng nhập: `admin@pcm.vn` / `Admin@123`
4. Xem Dashboard

---

## 🆘 TROUBLESHOOTING

### ❌ Lỗi: `Login failed for user 'NT AUTHORITY\ANONYMOUS LOGON'`

**Nguyên nhân:** Windows Auth không được cấu hình đúng

**Giải pháp:**
```bash
# Dùng SQL Server Auth thay vì Windows Auth
# Edit backend/API/appsettings.json:
"DefaultConnection": "Server=localhost;Database=020_PCM;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
```

### ❌ Lỗi: `Failed to connect to Redis`

**Nguyên nhân:** Redis chưa chạy

**Giải pháp:**
- Cài Redis từ https://github.com/microsoftarchive/redis/releases
- Hoặc dùng Docker: `docker run -d -p 6379:6379 redis`

### ❌ Lỗi: `Unable to resolve service for type IAuthService`

**Nguyên nhân:** DI container chưa được cấu hình

**Giải pháp:** Kiểm tra `Program.cs` có các dòng sau:
```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMemberService, MemberService>();
// ... etc
```

### ❌ Lỗi: `CORS error` khi gọi API từ Frontend

**Giải pháp:** Kiểm tra `Program.cs` có CORS policy:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

---

## 🐳 OPTION: DÙNG DOCKER (TÙY CHỌN)

Nếu bạn muốn chạy toàn bộ bằng Docker:

```bash
docker-compose up -d
```

Hệ thống sẽ tự động:
- ✅ Tạo SQL Server container
- ✅ Tạo Redis container
- ✅ Build & chạy Backend
- ✅ Build & chạy Frontend

**Truy cập:**
- Frontend: `http://localhost:3000`
- Backend: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`

---

## 📝 NOTES QUAN TRỌNG

1. **Database name:** `020_PCM` (dựa theo MSSV = 020)
2. **Admin:** `admin@pcm.vn` / `Admin@123`
3. **JWT Secret:** Đã được set trong `appsettings.json`, có thể thay bằng key dài hơn
4. **Redis:** Nếu không có Redis, tạm thời set `ICacheService` thành mock service
5. **Migrations:** Tự động chạy khi startup (xem `Program.cs`)

---

## ✨ TÍNH NĂNG READY

Bạn có thể test ngay:
- 📝 **Đăng ký/Đăng nhập** (JWT + Refresh Token)
- 💰 **E-Wallet** (Deposit, Balance checking)
- 📅 **Smart Booking** (Concurrency control, Refunds)
- 🏆 **ELO Rating** (Auto-update sau match)
- 📊 **Leaderboard** (Redis Sorted Sets)
- 🔐 **Authorization** (Role-based, Jwt Bearer)

---

## 🎓 NEXT STEPS

Nếu muốn thêm 100%:
1. **SignalR Hubs** - Real-time booking updates
2. **Hangfire Jobs** - Background jobs
3. **Tournament Features** - Bracket generation, Match management
4. **Frontend Views** - Calendar, Tournament UI

> Tất cả code templates có sẵn trong `IMPLEMENTATION_GUIDE.md`

---

**Chúc bạn thành công! 🚀**
