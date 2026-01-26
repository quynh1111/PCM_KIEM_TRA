using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PCM.Domain.Entities;
using PCM.Domain.Enums;
using PCM.Domain.Interfaces;
using PCM.Application.Interfaces;

namespace PCM.Infrastructure.Services
{
    public class DataSeeder
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITournamentService _tournamentService;

        public DataSeeder(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork unitOfWork,
            ITournamentService tournamentService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _tournamentService = tournamentService;
        }

        public async Task SeedAsync()
        {
            await EnsureRoleAsync("Admin");
            await EnsureRoleAsync("Treasurer");
            await EnsureRoleAsync("Member");

            var adminMember = await EnsureUserAsync(
                email: "admin@pcm.local",
                password: "Admin@123",
                fullName: "Quản trị viên",
                phoneNumber: "0900000000",
                role: "Admin",
                rankElo: 1500,
                walletBalance: 0m);

            var treasurerMember = await EnsureUserAsync(
                email: "treasurer@pcm.local",
                password: "Treasurer@123",
                fullName: "Thủ quỹ",
                phoneNumber: "0900000001",
                role: "Treasurer",
                rankElo: 1300,
                walletBalance: 0m);

            var member1 = await EnsureUserAsync(
                email: "member1@pcm.local",
                password: "Member@123",
                fullName: "Thành viên 1",
                phoneNumber: "0900000002",
                role: "Member",
                rankElo: 1250,
                walletBalance: 0m);

            var member2 = await EnsureUserAsync(
                email: "member2@pcm.local",
                password: "Member@123",
                fullName: "Thành viên 2",
                phoneNumber: "0900000003",
                role: "Member",
                rankElo: 1180,
                walletBalance: 0m);

            var member3 = await EnsureUserAsync(
                email: "member3@pcm.local",
                password: "Member@123",
                fullName: "Thành viên 3",
                phoneNumber: "0900000004",
                role: "Member",
                rankElo: 1150,
                walletBalance: 0m);

            var member4 = await EnsureUserAsync(
                email: "member4@pcm.local",
                password: "Member@123",
                fullName: "Thành viên 4",
                phoneNumber: "0900000005",
                role: "Member",
                rankElo: 1120,
                walletBalance: 0m);

            var member5 = await EnsureUserAsync(
                email: "member5@pcm.local",
                password: "Member@123",
                fullName: "Thành viên 5",
                phoneNumber: "0900000006",
                role: "Member",
                rankElo: 1100,
                walletBalance: 0m);

            var member6 = await EnsureUserAsync(
                email: "member6@pcm.local",
                password: "Member@123",
                fullName: "Thành viên 6",
                phoneNumber: "0900000007",
                role: "Member",
                rankElo: 1080,
                walletBalance: 0m);

            var members = new[] { member1, member2, member3, member4, member5, member6 };

            await EnsureCategoryAsync("Deposit", TransactionType.Income, TransactionScope.Wallet);
            await EnsureCategoryAsync("PayBooking", TransactionType.Expense, TransactionScope.Wallet);
            await EnsureCategoryAsync("PayTournament", TransactionType.Expense, TransactionScope.Wallet);
            await EnsureCategoryAsync("Refund", TransactionType.Income, TransactionScope.Wallet);

            await EnsureCategoryAsync("TreasuryIncome", TransactionType.Income, TransactionScope.Treasury);
            await EnsureCategoryAsync("TreasuryExpense", TransactionType.Expense, TransactionScope.Treasury);

            await EnsureCourtsAsync();

            var categories = (await _unitOfWork.TransactionCategories.GetAllAsync()).ToList();
            var categoryMap = categories.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);

            await EnsureTreasuryTransactionsAsync(adminMember, categoryMap);
            await EnsureWalletTransactionsAsync(member1, categoryMap, includePending: false);
            await EnsureWalletTransactionsAsync(member2, categoryMap, includePending: true);
            foreach (var member in members)
            {
                await EnsureMinimumWalletBalanceAsync(member, categoryMap, 1000000m);
            }
            await EnsureBookingsAsync(members);
            await EnsureTournamentsAsync(adminMember, members);
            await EnsureNewsAsync(adminMember);
        }

        private async Task EnsureRoleAsync(string name)
        {
            if (!await _roleManager.RoleExistsAsync(name))
            {
                await _roleManager.CreateAsync(new IdentityRole(name));
            }
        }

        private async Task EnsureCategoryAsync(string name, TransactionType type, TransactionScope scope)
        {
            var existing = (await _unitOfWork.TransactionCategories.FindAsync(c =>
                c.Name == name &&
                c.Type == type.ToString() &&
                c.Scope == scope.ToString()))
                .FirstOrDefault();

            if (existing != null)
                return;

            var category = new TransactionCategory
            {
                Id = Guid.NewGuid(),
                Name = name,
                Type = type.ToString(),
                Scope = scope.ToString(),
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.TransactionCategories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<Member> EnsureUserAsync(
            string email,
            string password,
            string fullName,
            string phoneNumber,
            string role,
            double rankElo,
            decimal walletBalance)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                await _userManager.CreateAsync(user, password);
            }

            if (!await _userManager.IsInRoleAsync(user, role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            var member = (await _unitOfWork.Members.FindAsync(m => m.UserId == user.Id)).FirstOrDefault();
            if (member == null)
            {
                member = new Member
                {
                    UserId = user.Id,
                    FullName = fullName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    JoinDate = DateTime.UtcNow,
                    RankELO = rankElo,
                    WalletBalance = walletBalance,
                    IsActive = true
                };
                await _unitOfWork.Members.AddAsync(member);
                await _unitOfWork.SaveChangesAsync();
            }

            return member;
        }

        private async Task EnsureCourtsAsync()
        {
            var existing = await _unitOfWork.Courts.GetAllAsync();
            if (existing.Any())
                return;

            var courts = new[]
            {
                new Court { Name = "Sân 1", Description = "Sân tiêu chuẩn", HourlyRate = 100000m, IsActive = true },
                new Court { Name = "Sân 2", Description = "Sân có mái che", HourlyRate = 120000m, IsActive = true },
                new Court { Name = "Sân 3", Description = "Sân premium", HourlyRate = 150000m, IsActive = true }
            };

            await _unitOfWork.Courts.AddRangeAsync(courts);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task EnsureTreasuryTransactionsAsync(Member adminMember, IDictionary<string, TransactionCategory> categoryMap)
        {
            var existing = await _unitOfWork.TreasuryTransactions.GetAllAsync();
            if (existing.Any())
                return;

            if (!categoryMap.TryGetValue("TreasuryIncome", out var incomeCat) ||
                !categoryMap.TryGetValue("TreasuryExpense", out var expenseCat))
                return;

            var now = DateTime.UtcNow;
            var transactions = new[]
            {
                new TreasuryTransaction
                {
                    Id = Guid.NewGuid(),
                    Date = now.AddDays(-3),
                    Amount = 2000000m,
                    Description = "Thu hội phí tháng",
                    CategoryId = incomeCat.Id,
                    CreatedByMemberId = adminMember.UserId,
                    CreatedDate = now.AddDays(-3)
                },
                new TreasuryTransaction
                {
                    Id = Guid.NewGuid(),
                    Date = now.AddDays(-2),
                    Amount = -500000m,
                    Description = "Chi phí bảo trì sân",
                    CategoryId = expenseCat.Id,
                    CreatedByMemberId = adminMember.UserId,
                    CreatedDate = now.AddDays(-2)
                },
                new TreasuryTransaction
                {
                    Id = Guid.NewGuid(),
                    Date = now.AddDays(-1),
                    Amount = 1200000m,
                    Description = "Thu phí giải đấu mini",
                    CategoryId = incomeCat.Id,
                    CreatedByMemberId = adminMember.UserId,
                    CreatedDate = now.AddDays(-1)
                }
            };

            await _unitOfWork.TreasuryTransactions.AddRangeAsync(transactions);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task EnsureWalletTransactionsAsync(
            Member member,
            IDictionary<string, TransactionCategory> categoryMap,
            bool includePending)
        {
            var existing = await _unitOfWork.WalletTransactions.FindAsync(t => t.MemberId == member.Id);
            if (existing.Any())
            {
                var currentBalance = existing
                    .Where(t => t.Status == WalletTransactionStatus.Success)
                    .Sum(t => t.Amount);

                if (member.WalletBalance != currentBalance)
                {
                    member.WalletBalance = currentBalance;
                    _unitOfWork.Members.Update(member);
                    await _unitOfWork.SaveChangesAsync();
                }

                return;
            }

            if (!categoryMap.TryGetValue("Deposit", out var depositCat) ||
                !categoryMap.TryGetValue("PayBooking", out var payCat) ||
                !categoryMap.TryGetValue("Refund", out var refundCat))
                return;

            var now = DateTime.UtcNow;
            var transactions = new List<WalletTransaction>
            {
                new WalletTransaction
                {
                    MemberId = member.Id,
                    Date = now.AddDays(-5),
                    Amount = 2000000m,
                    CategoryId = depositCat.Id,
                    Type = WalletTransactionType.Deposit,
                    ReferenceId = $"SEED-DEP-{member.Id}",
                    Description = "Nạp ví lần đầu",
                    Status = WalletTransactionStatus.Success,
                    ProofImageUrl = "https://via.placeholder.com/400x300"
                },
                new WalletTransaction
                {
                    MemberId = member.Id,
                    Date = now.AddDays(-2),
                    Amount = -500000m,
                    CategoryId = payCat.Id,
                    Type = WalletTransactionType.PayBooking,
                    ReferenceId = $"SEED-PAY-{member.Id}",
                    Description = "Thanh toán đặt sân",
                    Status = WalletTransactionStatus.Success
                },
                new WalletTransaction
                {
                    MemberId = member.Id,
                    Date = now.AddDays(-1),
                    Amount = 100000m,
                    CategoryId = refundCat.Id,
                    Type = WalletTransactionType.Refund,
                    ReferenceId = $"SEED-REF-{member.Id}",
                    Description = "Hoàn tiền hủy sân",
                    Status = WalletTransactionStatus.Success
                }
            };

            if (includePending)
            {
                transactions.Add(new WalletTransaction
                {
                    MemberId = member.Id,
                    Date = now,
                    Amount = 300000m,
                    CategoryId = depositCat.Id,
                    Type = WalletTransactionType.Deposit,
                    ReferenceId = $"SEED-PENDING-{member.Id}",
                    Description = "Nạp ví đang chờ duyệt",
                    Status = WalletTransactionStatus.Pending,
                    ProofImageUrl = "https://via.placeholder.com/400x300"
                });
            }

            foreach (var transaction in transactions)
            {
                transaction.EncryptedSignature = ComputeSignature(transaction);
            }

            await _unitOfWork.WalletTransactions.AddRangeAsync(transactions);

            var balance = transactions
                .Where(t => t.Status == WalletTransactionStatus.Success)
                .Sum(t => t.Amount);

            member.WalletBalance = balance;
            _unitOfWork.Members.Update(member);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task EnsureMinimumWalletBalanceAsync(
            Member member,
            IDictionary<string, TransactionCategory> categoryMap,
            decimal minimumBalance)
        {
            if (!categoryMap.TryGetValue("Deposit", out var depositCat))
                return;

            var transactions = await _unitOfWork.WalletTransactions.FindAsync(t =>
                t.MemberId == member.Id && t.Status == WalletTransactionStatus.Success);

            var balance = transactions.Sum(t => t.Amount);
            if (balance >= minimumBalance)
            {
                if (member.WalletBalance != balance)
                {
                    member.WalletBalance = balance;
                    _unitOfWork.Members.Update(member);
                    await _unitOfWork.SaveChangesAsync();
                }
                return;
            }

            var topUp = minimumBalance - balance;
            var transaction = new WalletTransaction
            {
                MemberId = member.Id,
                Date = DateTime.UtcNow,
                Amount = topUp,
                CategoryId = depositCat.Id,
                Type = WalletTransactionType.Deposit,
                ReferenceId = $"SEED-TOPUP-{member.Id}",
                Description = "Nạp ví bổ sung cho demo",
                Status = WalletTransactionStatus.Success
            };
            transaction.EncryptedSignature = ComputeSignature(transaction);

            await _unitOfWork.WalletTransactions.AddAsync(transaction);

            member.WalletBalance = balance + topUp;
            _unitOfWork.Members.Update(member);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task EnsureBookingsAsync(IEnumerable<Member> members)
        {
            var existing = await _unitOfWork.Bookings.GetAllAsync();
            if (existing.Any())
                return;

            var courts = (await _unitOfWork.Courts.GetAllAsync()).ToList();
            var court1 = courts.FirstOrDefault();
            var court2 = courts.Skip(1).FirstOrDefault();
            if (court1 == null || court2 == null)
                return;

            var memberList = members.ToList();
            if (memberList.Count < 2)
                return;

            var tomorrow = DateTime.Today.AddDays(1);
            var bookings = new[]
            {
                new Booking
                {
                    CourtId = court1.Id,
                    MemberId = memberList[0].Id,
                    StartTime = tomorrow.AddHours(17),
                    EndTime = tomorrow.AddHours(18),
                    TotalPrice = court1.HourlyRate,
                    Status = BookingStatus.Confirmed,
                    Note = "Đặt sân tập luyện"
                },
                new Booking
                {
                    CourtId = court2.Id,
                    MemberId = memberList[1].Id,
                    StartTime = tomorrow.AddHours(18),
                    EndTime = tomorrow.AddHours(19),
                    TotalPrice = court2.HourlyRate,
                    Status = BookingStatus.Confirmed,
                    Note = "Đặt sân giao hữu"
                },
                new Booking
                {
                    CourtId = court1.Id,
                    MemberId = memberList[0].Id,
                    StartTime = tomorrow.AddDays(-1).AddHours(19),
                    EndTime = tomorrow.AddDays(-1).AddHours(20),
                    TotalPrice = court1.HourlyRate,
                    Status = BookingStatus.Cancelled,
                    Note = "Đã hủy"
                }
            };

            await _unitOfWork.Bookings.AddRangeAsync(bookings);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task EnsureTournamentsAsync(Member adminMember, IEnumerable<Member> members)
        {
            var existing = (await _unitOfWork.Tournaments.GetAllAsync()).ToList();

            var knockout = existing.FirstOrDefault(t => t.Name == "PCM Knockout Cup");
            if (knockout == null)
            {
                knockout = new Tournament
                {
                    Name = "PCM Knockout Cup",
                    Description = "Giải đấu loại trực tiếp",
                    StartDate = DateTime.UtcNow.AddDays(7),
                    EndDate = DateTime.UtcNow.AddDays(14),
                    Type = TournamentType.Professional,
                    Format = TournamentFormat.Knockout,
                    Status = TournamentStatus.Open,
                    EntryFee = 100000m,
                    PrizePool = 1000000m,
                    MaxParticipants = 8,
                    RegistrationDeadline = DateTime.UtcNow.AddDays(5),
                    CreatedBy = adminMember.UserId,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.Tournaments.AddAsync(knockout);
                await _unitOfWork.SaveChangesAsync();
            }

            var openTournament = existing.FirstOrDefault(t => t.Name == "PCM Open Series");
            if (openTournament == null)
            {
                openTournament = new Tournament
                {
                    Name = "PCM Open Series",
                    Description = "Giải đấu mở cho thành viên đăng ký mới",
                    StartDate = DateTime.UtcNow.AddDays(3),
                    EndDate = DateTime.UtcNow.AddDays(10),
                    Type = TournamentType.MiniGame,
                    Format = TournamentFormat.RoundRobin,
                    Status = TournamentStatus.Open,
                    EntryFee = 50000m,
                    PrizePool = 0m,
                    MaxParticipants = 16,
                    RegistrationDeadline = DateTime.UtcNow.AddDays(2),
                    CreatedBy = adminMember.UserId,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.Tournaments.AddAsync(openTournament);
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                var now = DateTime.UtcNow;
                var updated = false;

                if (openTournament.RegistrationDeadline.HasValue &&
                    openTournament.RegistrationDeadline.Value < now.AddDays(1))
                {
                    openTournament.RegistrationDeadline = now.AddDays(2);
                    updated = true;
                }

                if (openTournament.StartDate < now.AddDays(1))
                {
                    openTournament.StartDate = now.AddDays(3);
                    updated = true;
                }

                if (openTournament.EndDate <= openTournament.StartDate)
                {
                    openTournament.EndDate = openTournament.StartDate.AddDays(7);
                    updated = true;
                }

                if (openTournament.Status != TournamentStatus.Open)
                {
                    openTournament.Status = TournamentStatus.Open;
                    updated = true;
                }

                if (updated)
                {
                    _unitOfWork.Tournaments.Update(openTournament);
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            if (knockout != null)
            {
                var participants = (await _unitOfWork.Participants.FindAsync(p => p.TournamentId == knockout.Id)).ToList();
                if (!participants.Any())
                {
                    foreach (var member in members.Take(knockout.MaxParticipants > 0 ? knockout.MaxParticipants : 8))
                    {
                        var participant = new Participant
                        {
                            TournamentId = knockout.Id,
                            MemberId = member.Id,
                            TeamName = member.FullName,
                            Status = ParticipantStatus.Paid,
                            RegisteredDate = DateTime.UtcNow
                        };
                        await _unitOfWork.Participants.AddAsync(participant);
                    }

                    await _unitOfWork.SaveChangesAsync();
                }

                await _tournamentService.GenerateBracketAsync(knockout.Id);
            }
        }

        private async Task EnsureNewsAsync(Member adminMember)
        {
            var existing = await _unitOfWork.News.GetAllAsync();
            if (existing.Any())
                return;

            var items = new[]
            {
                new News
                {
                    Title = "Lịch nghỉ Tết",
                    Content = "CLB nghỉ từ 28/01 đến 02/02. Hẹn gặp lại các thành viên!",
                    IsPinned = true,
                    CreatedBy = adminMember.UserId,
                    CreatedDate = DateTime.UtcNow.AddDays(-2)
                },
                new News
                {
                    Title = "Vinh danh top ELO",
                    Content = "Chúc mừng thành viên có thứ hạng ELO cao nhất tháng này.",
                    IsPinned = false,
                    CreatedBy = adminMember.UserId,
                    CreatedDate = DateTime.UtcNow.AddDays(-1)
                }
            };

            await _unitOfWork.News.AddRangeAsync(items);
            await _unitOfWork.SaveChangesAsync();
        }

        private static string ComputeSignature(WalletTransaction transaction)
        {
            var input = $"{transaction.MemberId}|{transaction.Amount}|{transaction.Date:O}|{transaction.ReferenceId}|{transaction.Status}";
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}
