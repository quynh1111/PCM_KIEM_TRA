# 🏆 HỆ THỐNG QUẢN LÝ CLB PICKLEBALL "VỢT THỦ PHỐ NÚI" (PCM)
## PRO EDITION - CLEAN ARCHITECTURE

![Status](https://img.shields.io/badge/Status-90%25%20Complete-brightgreen)
![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-blue)
![Tech](https://img.shields.io/badge/Tech-.NET%208%20%7C%20Vue%203%20%7C%20Redis%20%7C%20Docker-orange)

> **Dự án Fullstack Advanced** - Hệ thống quản lý CLB Pickleball chuyên nghiệp với Clean Architecture, Concurrency Control, Real-time features, và Docker deployment.

---

## 🎯 TỔNG QUAN

Đây là dự án **hoàn chỉnh** theo đề bài **Mota_Fullstack_Advanced.md** với đầy đủ:
- ✅ Clean Architecture (Domain, Application, Infrastructure, API)
- ✅ E-Wallet System với Transaction Safety
- ✅ Smart Booking với Optimistic Locking
- ✅ ELO Rating System
- ✅ Redis Caching & Leaderboard
- ✅ JWT Authentication
- ✅ Docker Deployment
- 📝 SignalR & Hangfire (code templates sẵn sàng)

**📊 Tiến độ: 90%** - Sẵn sàng chạy và demo!

---

## 📚 TÀI LIỆU

| File | Mô tả | Dành cho |
|------|-------|----------|
| **[QUICK_START.md](QUICK_START.md)** | Hướng dẫn bắt đầu nhanh (5 phút) | ⭐ BẮT ĐẦU TẠI ĐÂY |
| **[COMPLETION_REPORT.md](COMPLETION_REPORT.md)** | Báo cáo hoàn thành dự án | Tổng quan chi tiết |
| **[PROJECT_SUMMARY.md](PROJECT_SUMMARY.md)** | Tổng kết những gì đã làm | Review tiến độ |
| **[IMPLEMENTATION_GUIDE.md](IMPLEMENTATION_GUIDE.md)** | Code templates còn thiếu | Hoàn thiện thêm |
| **[README_FULL.md](README_FULL.md)** | Tài liệu kỹ thuật đầy đủ | Tham khảo chi tiết |

---

## ⚡ QUICK START (3 BƯỚC)

### Bước 1: Chuẩn bị
```bash
# Kiểm tra môi trường
dotnet --version    # Cần .NET 8.0+
node --version      # Cần Node.js 18+
docker --version    # Khuyến nghị

# Clone project (nếu từ Git)
git clone <your-repo>
cd pcm_advplus_patch
```

### Bước 2: Thay thế Student ID
Tìm và thay thế **`XXX`** bằng **3 số cuối MSSV** trong:
- `backend/Infrastructure/Persistence/Configurations/*.cs`
  - Ví dụ: `XXX_Members` → `123_Members`

### Bước 3: Chạy bằng Docker (Đơn giản nhất)
```bash
docker-compose up -d

# Chờ 1-2 phút, sau đó:
# Frontend: http://localhost:3000
# Backend: http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

**🎉 Done! Đăng nhập với: `admin@pcm.vn` / `Admin@123`**

> **Chi tiết hơn?** → Xem [QUICK_START.md](QUICK_START.md)

---

## 🏗️ KIẾN TRÚC DỰ ÁN

```
┌─────────────────────────────────────────────────────┐
│                  Frontend (Vue 3)                   │
│  ┌──────────┬───────────┬──────────┬──────────┐    │
│  │ Views    │ Stores    │ Router   │ API      │    │
│  │ (Login,  │ (Pinia)   │ (Guards) │ (Axios)  │    │
│  │  Dash,   │           │          │          │    │
│  │  Booking)│           │          │          │    │
│  └──────────┴───────────┴──────────┴──────────┘    │
└─────────────────────────────────────────────────────┘
                         ↕ HTTP/SignalR
┌─────────────────────────────────────────────────────┐
│            Backend (ASP.NET Core 8.0)               │
│  ┌──────────────────────────────────────────────┐  │
│  │  API Layer (Controllers, Hubs, Middleware)   │  │
│  └──────────────────────────────────────────────┘  │
│                         ↓                           │
│  ┌──────────────────────────────────────────────┐  │
│  │  Application Layer (Services, DTOs)          │  │
│  │  • AuthService      • MemberService          │  │
│  │  • WalletService    • BookingService         │  │
│  │  • TournamentSvc    • MatchService           │  │
│  └──────────────────────────────────────────────┘  │
│                         ↓                           │
│  ┌──────────────────────────────────────────────┐  │
│  │  Infrastructure Layer                        │  │
│  │  • EF Core DbContext  • Redis Cache          │  │
│  │  • Repository/UoW     • SignalR Hubs         │  │
│  │  • Hangfire Jobs                             │  │
│  └──────────────────────────────────────────────┘  │
│                         ↓                           │
│  ┌──────────────────────────────────────────────┐  │
│  │  Domain Layer (Entities, Interfaces)         │  │
│  │  • Member  • Booking  • Tournament           │  │
│  │  • Match   • Court    • Wallet               │  │
│  └──────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────┘
           ↓                    ↓
    ┌──────────┐         ┌──────────┐
    │ SQL      │         │ Redis    │
    │ Server   │         │ Cache    │
    └──────────┘         └──────────┘
```

---

## 🔥 TÍNH NĂNG NỔI BẬT

### 1. 💰 E-Wallet System
- **Thread-safe** balance operations
- **Transaction integrity** (Begin/Commit/Rollback)
- **SHA256 signature** cho mỗi giao dịch
- Deposit request → Admin approval flow
- Refund policy thông minh

### 2. 📅 Smart Booking System
- **Optimistic Locking** với RowVersion
- **Recurring Booking** với conflict detection
- **Race Condition Prevention**
- Auto-price calculation
- Refund: 100%/50%/0% dựa trên thời gian

### 3. 🏆 Tournament & ELO Rating
- **Standard ELO algorithm**
- Team rating cho doubles
- Bracket generation (Knockout)
- Auto-rank update sau match

### 4. ⚡ Redis Caching
- Generic cache service
- **Sorted Sets** cho leaderboard
- Cache invalidation strategy
- Sub-second leaderboard queries

### 5. 🔐 JWT Authentication
- Access + Refresh token
- Auto-refresh flow trong frontend
- Token revocation
- Role-based authorization

### 6. 🐳 Docker Deployment
- Multi-stage builds (tối ưu size)
- Health checks
- Volume persistence
- 1-command deployment

---

## 📊 CHI TIẾT KỸ THUẬT

### Backend Stack
- **Framework**: ASP.NET Core 8.0 Web API
- **ORM**: Entity Framework Core 8.0
- **Database**: SQL Server 2022
- **Cache**: Redis 7 (StackExchange.Redis)
- **Auth**: ASP.NET Core Identity + JWT
- **Real-time**: SignalR
- **Background Jobs**: Hangfire
- **Architecture**: Clean Architecture

### Frontend Stack  
- **Framework**: Vue.js 3 (Composition API)
- **State**: Pinia
- **Router**: Vue Router 4
- **HTTP Client**: Axios
- **Build Tool**: Vite
- **Real-time**: SignalR Client

### DevOps
- **Containerization**: Docker & Docker Compose
- **Web Server**: Nginx (Frontend)
- **Orchestration**: Docker Compose with health checks

---

## 📁 CẤU TRÚC FILE

<details>
<summary><b>Xem chi tiết (click to expand)</b></summary>

```
pcm_advplus_patch/
├── 📚 DOCS/
│   ├── README.md                           # File này
│   ├── QUICK_START.md                      # ⭐ Bắt đầu tại đây
│   ├── COMPLETION_REPORT.md                # Báo cáo hoàn thành
│   ├── PROJECT_SUMMARY.md                  # Tổng kết
│   ├── IMPLEMENTATION_GUIDE.md             # Code templates
│   └── README_FULL.md                      # Tài liệu đầy đủ
│
├── 🎯 BACKEND/
│   ├── Domain/                             # 30+ files
│   │   ├── Entities/                       # ✅ 12 entities
│   │   ├── Enums/                          # ✅ 13 enums
│   │   └── Interfaces/                     # ✅ 3 interfaces
│   │
│   ├── Application/                        # 30+ files
│   │   ├── DTOs/                           # ✅ 25+ DTOs
│   │   └── Interfaces/                     # ✅ 7 services
│   │
│   ├── Infrastructure/                     # 15+ files
│   │   ├── Persistence/
│   │   │   ├── PCMDbContext.cs             # ✅
│   │   │   ├── Configurations/             # ✅ 5 configs
│   │   │   └── Repositories/               # ✅ UoW pattern
│   │   └── Services/                       # ✅ 6 services
│   │
│   └── API/
│       ├── Controllers/                    # ✅ 3 controllers
│       ├── appsettings.json                # ✅
│       └── Dockerfile                      # ✅
│
├── 🎨 FRONTEND/
│   ├── src/
│   │   ├── views/                          # ✅ 7 views
│   │   ├── stores/                         # ✅ Auth store
│   │   ├── router/                         # ✅ Routes + guards
│   │   ├── api/                            # ✅ HTTP client
│   │   └── assets/                         # ✅ Styles
│   ├── package.json                        # ✅
│   ├── vite.config.js                      # ✅
│   └── Dockerfile                          # ✅
│
└── 🐳 DOCKER/
    ├── docker-compose.yml                  # ✅ 4 services
    └── .gitignore                          # ✅
```

</details>

---

## 🧪 TESTING

### Test Authentication
1. Vào http://localhost:3000/register
2. Đăng ký tài khoản mới
3. Đăng nhập và xem Dashboard

### Test API (Swagger)
1. Vào http://localhost:5000/swagger
2. POST `/api/auth/login` → Lấy token
3. Click "Authorize" → Paste token
4. Test các endpoints

### Test Booking Flow
1. GET `/api/bookings/available-slots?date=2026-01-25`
2. POST `/api/bookings` → Tạo booking
3. Kiểm tra ví bị trừ tiền
4. PUT `/api/bookings/{id}/cancel` → Test refund



---

## 🆘 HỖ TRỢ & TROUBLESHOOTING

### Lỗi thường gặp

**Migration failed:**
```bash
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project API
```

**Connection refused:**
- Kiểm tra SQL Server & Redis đang chạy
- Kiểm tra connection string trong appsettings.json

**Frontend không connect API:**
- Kiểm tra backend đang chạy tại port 5000
- Kiểm tra CORS settings

> **Chi tiết hơn?** → Xem [QUICK_START.md](QUICK_START.md) section Troubleshooting

---

## 👨‍💻 TÁC GIẢ

- **Sinh viên**: [Đinh Trọng Quỳnh]
- **MSSV**: [020]
- **Lớp**: Fullstack Development
- **Năm**: 2026

---

## 📄 LICENSE

Educational project - For learning purposes only.

---

## 🙏 LỜI CẢM ƠN

Dự án được xây dựng theo đề bài **Mota_Fullstack_Advanced.md** với mục tiêu học tập và rèn luyện kỹ năng:
- Clean Architecture
- Domain-Driven Design
- Concurrency Control
- Real-time Applications
- Docker Deployment

**Chúc các bạn học tốt! 🎓**

---

<div align="center">

**⭐ Nếu bạn thấy dự án hữu ích, hãy cho một star! ⭐**

Made with ❤️ for PCM Club

</div>
- `GET /api/transactions/summary`
- `GET /api/transaction-categories`
- `POST /api/transaction-categories`
- `PUT /api/transaction-categories/{id}`
- `DELETE /api/transaction-categories/{id}`

Phân quyền: Admin, Treasurer.

### 2.2. Booking compatibility
- `GET /api/bookings/available-slots?courtId=&date=YYYY-MM-DD`
Gọi chung logic slot của advanced (vd `/api/bookings/slots`).

### 2.3. Members top ranking
- `GET /api/members/top-ranking?limit=5`

### 2.4. Challenges compatibility (alias)
Bản thường dùng `/api/challenges`. Patch thêm controller alias:
- `GET /api/challenges`
- `GET /api/challenges/{id}`
- `POST /api/challenges`
- `PUT /api/challenges/{id}`
- `POST /api/challenges/{id}/join`
- `POST /api/challenges/{id}/auto-divide-teams`

Nội bộ sẽ gọi Tournaments service của bạn.

---

## 3) Frontend (tuỳ chọn)
Trong `frontend/` có skeleton Vue 3 cho:
- Dashboard: pinned news + treasury summary + top 5 ranking
- Treasury pages: categories + transactions

Bạn có thể copy và chỉnh router/store theo project.

---

## 4) Seed checklist (đề thường)
- 1 Admin + 6–8 members
- ≥ 2 courts
- Categories Thu/Chi (Scope=Treasure)
- 5–10 TreasuryTransactions (để có balance)
- 1 TeamBattle ongoing + participants 10–12
- 2–3 matches

---
