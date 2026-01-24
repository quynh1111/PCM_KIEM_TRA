# HỆ THỐNG QUẢN LÝ CLB PICKLEBALL "VỢT THỦ PHỐ NÚI" (PCM) - PRO EDITION

## 🎯 TỔNG QUAN DỰ ÁN

Hệ thống quản lý CLB Pickleball chuyên nghiệp với đầy đủ tính năng:
- ✅ **Authentication & Authorization** với ASP.NET Core Identity + JWT
- ✅ **E-Wallet System** - Ví điện tử nội bộ
- ✅ **Smart Booking** - Đặt sân thông minh với Concurrency Control
- ✅ **Tournament Management** - Quản lý giải đấu với Bracket System
- ✅ **ELO Rating System** - Hệ thống xếp hạng động
- ✅ **Redis Caching** - Tối ưu hiệu năng
- ✅ **SignalR Real-time** - Cập nhật thời gian thực
- ✅ **Hangfire Background Jobs** - Xử lý tác vụ nền
- ✅ **Docker Deployment** - Triển khai đóng gói

## 📁 CẤU TRÚC DỰ ÁN (Clean Architecture)

```
PCM_AdvancedPlus/
├── backend/
│   ├── Domain/              # Core business entities & interfaces
│   │   ├── Entities/        # Domain entities (Member, Booking, Tournament, etc.)
│   │   ├── Enums/           # Business enums
│   │   └── Interfaces/      # Repository & service interfaces
│   │
│   ├── Application/         # Use cases & business logic
│   │   ├── DTOs/            # Data Transfer Objects
│   │   ├── Interfaces/      # Service interfaces
│   │   └── Services/        # Service implementations
│   │
│   ├── Infrastructure/      # External concerns
│   │   ├── Persistence/     # EF Core DbContext & Configurations
│   │   ├── Services/        # Redis, Email, SignalR implementations
│   │   └── Identity/        # Identity configuration
│   │
│   └── API/                 # Presentation layer
│       ├── Controllers/     # API endpoints
│       ├── Hubs/            # SignalR hubs
│       ├── Middleware/      # Error handling, logging
│       └── Filters/         # Action filters
│
├── frontend/                # Vue.js 3 + Pinia
│   ├── src/
│   │   ├── api/             # API service layer
│   │   ├── components/      # Reusable components
│   │   ├── views/           # Page components
│   │   ├── stores/          # Pinia stores
│   │   ├── router/          # Vue Router
│   │   └── utils/           # Utilities & helpers
│   │
│   └── public/              # Static assets
│
└── docker/                  # Docker configurations
    ├── docker-compose.yml
    ├── Dockerfile.backend
    └── Dockerfile.frontend
```

## 🗄️ DATABASE SCHEMA

### Bảng chính (Prefix: XXX_ = 3 số cuối MSSV)

#### 1. Identity & Members
- `XXX_Members`: Hồ sơ thành viên + Ví
- `XXX_RefreshTokens`: JWT refresh tokens
- `XXX_News`: Tin tức & thông báo

#### 2. Treasury & Wallet
- `XXX_TransactionCategories`: Danh mục giao dịch
- `XXX_TreasuryTransactions`: Giao dịch quỹ CLB
- `XXX_WalletTransactions`: Giao dịch ví cá nhân

#### 3. Booking System
- `XXX_Courts`: Danh sách sân
- `XXX_Bookings`: Đơn đặt sân (có RowVersion cho Concurrency)

#### 4. Tournament System
- `XXX_Tournaments`: Thông tin giải đấu
- `XXX_Participants`: Người tham gia
- `XXX_Matches`: Trận đấu
- `XXX_TournamentMatches`: Cây đấu bracket

## 🚀 HƯỚNG DẪN SETUP

### Prerequisites
- .NET 8.0 SDK
- Node.js 18+
- SQL Server 2019+
- Redis 7+
- Docker Desktop (optional)

### 1. Backend Setup

```bash
cd backend

# Cài đặt packages (sẽ được tự động restore)
dotnet restore

# Cấu hình connection string
# Edit appsettings.json - thay đổi connection string

# Tạo migration
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project API

# Update database
dotnet ef database update --project Infrastructure --startup-project API

# Chạy backend
dotnet run --project API
```

### 2. Frontend Setup

```bash
cd frontend

# Cài đặt dependencies
npm install

# Chạy development server
npm run dev

# Build production
npm run build
```

### 3. Docker Setup (Khuyến nghị)

```bash
# Chạy toàn bộ hệ thống
docker-compose up -d

# Xem logs
docker-compose logs -f

# Dừng hệ thống
docker-compose down
```

## ⚙️ CẤU HÌNH QUAN TRỌNG

### appsettings.json (Backend)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PCM_Advanced;User Id=sa;Password=YourPassword;TrustServerCertificate=true"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKey_AtLeast32Characters",
    "Issuer": "PCM_API",
    "Audience": "PCM_Client",
    "AccessTokenExpiration": 60,
    "RefreshTokenExpiration": 10080
  },
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "PCM:"
  },
  "Hangfire": {
    "DashboardPath": "/hangfire",
    "Username": "admin",
    "Password": "admin123"
  }
}
```

### .env (Frontend)

```env
VITE_API_BASE_URL=http://localhost:5000/api
VITE_SIGNALR_HUB_URL=http://localhost:5000/hubs
```

## 🔑 TÀI KHOẢN MẪU

Sau khi chạy seed data:

### Admin Account
- Email: admin@pcm.vn
- Password: Admin@123

### Member Accounts
- Email: member1@pcm.vn
- Password: Member@123

## 📋 API ENDPOINTS CHÍNH

### Authentication
- `POST /api/auth/login` - Đăng nhập
- `POST /api/auth/register` - Đăng ký
- `POST /api/auth/refresh-token` - Refresh token
- `POST /api/auth/revoke-token` - Logout

### Members
- `GET /api/members/me` - Profile cá nhân
- `PUT /api/members/profile` - Cập nhật profile
- `GET /api/members/top-ranking` - Top 5 ranking

### Wallet
- `GET /api/wallet/balance` - Số dư ví
- `POST /api/wallet/deposit` - Yêu cầu nạp tiền
- `POST /api/wallet/approve-deposit` - Duyệt nạp tiền (Admin)
- `GET /api/wallet/transactions` - Lịch sử giao dịch

### Bookings
- `GET /api/bookings/available-slots` - Lịch trống
- `POST /api/bookings` - Đặt sân đơn
- `POST /api/bookings/recurring` - Đặt sân định kỳ
- `PUT /api/bookings/{id}/cancel` - Hủy đặt sân
- `GET /api/bookings/my-bookings` - Lịch sử đặt sân

### Tournaments
- `GET /api/tournaments` - Danh sách giải
- `POST /api/tournaments` - Tạo giải (Admin)
- `POST /api/tournaments/{id}/join` - Đăng ký tham gia
- `GET /api/tournaments/{id}/bracket` - Xem cây đấu
- `POST /api/tournaments/{id}/generate-bracket` - Tạo cây đấu (Admin)

### Matches
- `POST /api/matches` - Tạo trận đấu
- `PUT /api/matches/{id}/result` - Cập nhật kết quả
- `GET /api/matches` - Danh sách trận đấu

## 🎨 FRONTEND FEATURES

### User Features
1. **Dashboard**: Tổng quan hoạt động
2. **Booking Calendar**: Lịch đặt sân trực quan
3. **My Wallet**: Quản lý ví điện tử
4. **Tournaments**: Xem và đăng ký giải đấu
5. **Leaderboard**: BXH realtime
6. **Profile**: Quản lý thông tin cá nhân

### Admin Features
1. **Member Management**: Quản lý thành viên
2. **Court Management**: Quản lý sân
3. **Tournament Management**: Quản lý giải đấu
4. **Financial Reports**: Báo cáo tài chính
5. **System Settings**: Cấu hình hệ thống

## 🔥 TÍNH NĂNG NỔI BẬT

### 1. Concurrency Control
- **Optimistic Locking**: Sử dụng RowVersion cho Booking
- **Transaction Management**: Unit of Work pattern
- **Race Condition Prevention**: Xử lý 2 người đặt cùng 1 sân

### 2. ELO Rating System
- Tự động tính toán sau mỗi trận đấu
- K-factor điều chỉnh theo độ chênh lệch
- Cập nhật real-time qua SignalR

### 3. Recurring Booking
- Đặt lịch định kỳ (hàng tuần)
- Phát hiện conflict tự động
- Cho phép skip ngày trùng

### 4. Tournament Bracket
- Tự động tạo cây đấu Knockout
- Hỗ trợ Single/Double Elimination
- Cập nhật real-time

### 5. Real-time Notifications
- SignalR Hub cho các sự kiện:
  - Booking state changes
  - Match score updates
  - Wallet transactions
  - Tournament updates

## 📊 BACKGROUND JOBS (Hangfire)

- **Auto-Cancel Pending Bookings**: Hủy booking pending > 15 phút
- **Daily Report**: Báo cáo doanh thu cuối ngày
- **Cache Refresh**: Làm mới cache định kỳ
- **Leaderboard Update**: Cập nhật BXH

## 🧪 TESTING

```bash
# Backend unit tests
cd backend
dotnet test

# Frontend unit tests
cd frontend
npm run test:unit

# E2E tests
npm run test:e2e
```

## 📝 CHECKLIST HOÀN THÀNH

### Backend ✅
- [x] Clean Architecture setup
- [x] Domain Entities & Enums
- [x] Repository Pattern & Unit of Work
- [x] Application Services (DTOs, Interfaces)
- [ ] Infrastructure Implementation
- [ ] API Controllers
- [ ] SignalR Hubs
- [ ] Hangfire Jobs
- [ ] Authentication & Authorization
- [ ] EF Core Configurations

### Frontend ⏳
- [ ] Project setup (Vite + Vue 3)
- [ ] Pinia stores
- [ ] API integration
- [ ] Authentication flow
- [ ] Booking calendar UI
- [ ] Tournament bracket visualization
- [ ] Real-time features
- [ ] Responsive design

### DevOps ⏳
- [ ] Docker configurations
- [ ] CI/CD pipeline
- [ ] Environment configurations

## 🐛 TROUBLESHOOTING

### Backend không kết nối SQL Server
```bash
# Kiểm tra connection string
# Đảm bảo SQL Server đang chạy
# Kiểm tra firewall
```

### Redis connection failed
```bash
# Kiểm tra Redis đang chạy
redis-cli ping
# PONG
```

### SignalR không hoạt động
```bash
# Kiểm tra CORS settings
# Kiểm tra WebSocket support
```

## 📚 TÀI LIỆU THAM KHẢO

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [EF Core Docs](https://docs.microsoft.com/en-us/ef/core/)
- [SignalR Docs](https://docs.microsoft.com/en-us/aspnet/core/signalr/)
- [Hangfire Docs](https://docs.hangfire.io/)
- [Vue.js 3 Docs](https://vuejs.org/)

## 👥 CONTRIBUTORS

- Sinh viên: [Tên của bạn]
- MSSV: [XXX]

## 📄 LICENSE

This project is for educational purposes only.

---

**Happy Coding! 🚀🎾**
