using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PCM.Application.DTOs.Bookings;
using PCM.Application.Interfaces;
using PCM.Domain.Entities;
using PCM.Domain.Enums;
using PCM.Domain.Interfaces;

namespace PCM.Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWalletService _walletService;
        private readonly ICacheService _cacheService;

        public BookingService(
            IUnitOfWork unitOfWork,
            IWalletService walletService,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _walletService = walletService;
            _cacheService = cacheService;
        }

        public async Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(DateTime date, int? courtId = null)
        {
            // Try cache first
            var cacheKey = $"booking:slots:{date:yyyy-MM-dd}:{courtId}";
            var cached = await _cacheService.GetAsync<List<AvailableSlotDto>>(cacheKey);
            if (cached != null)
                return cached;

            var courts = courtId.HasValue
                ? await _unitOfWork.Courts.FindAsync(c => c.Id == courtId.Value && c.IsActive)
                : await _unitOfWork.Courts.FindAsync(c => c.IsActive);

            var result = new List<AvailableSlotDto>();

            foreach (var court in courts)
            {
                var slots = await GenerateSlotsForDay(court, date);
                result.Add(new AvailableSlotDto
                {
                    CourtId = court.Id,
                    CourtName = court.Name,
                    Date = date,
                    AvailableSlots = slots
                });
            }

            // Cache for 2 minutes
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(2));

            return result;
        }

        public async Task<BookingDto> CreateBookingAsync(string userId, CreateBookingDto dto)
        {
            var member = await _unitOfWork.Members.FirstOrDefaultAsync(m => m.UserId == userId);
            if (member == null)
                throw new Exception("Không tìm thấy hội viên");

            var court = await _unitOfWork.Courts.GetByIdAsync(dto.CourtId);
            if (court == null || !court.IsActive)
                throw new Exception("Không tìm thấy sân hoặc sân đang ngưng hoạt động");

            if (dto.EndTime <= dto.StartTime)
                throw new Exception("Giờ kết thúc phải sau giờ bắt đầu");

            if (dto.StartTime < DateTime.Now)
                throw new Exception("Giờ bắt đầu phải ở tương lai");

            // CRITICAL: Start transaction for concurrency control
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Check for conflicts using database lock
                var hasConflict = await CheckTimeConflictAsync(dto.CourtId, dto.StartTime, dto.EndTime);
                if (hasConflict)
                    throw new Exception("Khung giờ đã có người đặt");

                // Calculate price
                var hours = (decimal)(dto.EndTime - dto.StartTime).TotalHours;
                var totalPrice = hours * court.HourlyRate;

                // Check wallet balance and deduct
                var deducted = await _walletService.DeductBalanceAsync(
                    member.Id,
                    totalPrice,
                    $"BOOKING-{Guid.NewGuid():N}",
                    $"Thanh toán đặt sân {court.Name} - {dto.StartTime:dd/MM/yyyy HH:mm}");

                if (!deducted)
                    throw new Exception("Số dư ví không đủ");

                // Create booking
                var booking = new Booking
                {
                    CourtId = dto.CourtId,
                    MemberId = member.Id,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    TotalPrice = totalPrice,
                    Status = BookingStatus.Confirmed,
                    Note = dto.Note,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.Bookings.AddAsync(booking);
                await _unitOfWork.CommitTransactionAsync();

                // Invalidate cache
                await InvalidateBookingCache(dto.CourtId, dto.StartTime);

                return MapToDto(booking, court.Name, member.FullName);
            }
            catch (DbUpdateConcurrencyException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception("Xung đột đặt sân, vui lòng thử lại.");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<RecurringBookingResultDto> CreateRecurringBookingAsync(string userId, CreateRecurringBookingDto dto)
        {
            var member = await _unitOfWork.Members.FirstOrDefaultAsync(m => m.UserId == userId);
            if (member == null)
                throw new Exception("Không tìm thấy hội viên");

            var court = await _unitOfWork.Courts.GetByIdAsync(dto.CourtId);
            if (court == null || !court.IsActive)
                throw new Exception("Không tìm thấy sân hoặc sân đang ngưng hoạt động");

            if (dto.StartDate.Date > dto.EndDate.Date)
                throw new Exception("Ngày bắt đầu phải trước ngày kết thúc");

            if (dto.EndTime <= dto.StartTime)
                throw new Exception("Giờ kết thúc phải sau giờ bắt đầu");

            if (dto.DaysOfWeek == null || dto.DaysOfWeek.Count == 0)
                throw new Exception("Vui lòng chọn thứ trong tuần");

            var result = new RecurringBookingResultDto();
            var recurringGroupId = Guid.NewGuid().ToString();
            var remainingBalance = await _walletService.GetBalanceAsync(userId);

            // Generate all dates
            var dates = GenerateRecurringDates(dto.StartDate, dto.EndDate, dto.DaysOfWeek);
            result.TotalAttempted = dates.Count;

            foreach (var date in dates)
            {
                try
                {
                    var startDateTime = date.Add(dto.StartTime);
                    var endDateTime = date.Add(dto.EndTime);

                    if (startDateTime < DateTime.Now)
                    {
                        result.Conflicts.Add(new ConflictDto
                        {
                            Date = date,
                            Reason = "Giờ bắt đầu phải ở tương lai"
                        });
                        result.TotalFailed++;
                        continue;
                    }

                    // Check conflict
                    var hasConflict = await CheckTimeConflictAsync(dto.CourtId, startDateTime, endDateTime);
                    if (hasConflict)
                    {
                        result.Conflicts.Add(new ConflictDto
                        {
                            Date = date,
                            Reason = "Khung giờ đã có người đặt"
                        });
                        result.TotalFailed++;
                        continue;
                    }

                    // Calculate price
                    var hours = (decimal)(endDateTime - startDateTime).TotalHours;
                    var totalPrice = hours * court.HourlyRate;

                    // Check balance
                    if (remainingBalance < totalPrice)
                    {
                        result.Conflicts.Add(new ConflictDto
                        {
                            Date = date,
                            Reason = "Số dư ví không đủ"
                        });
                        result.TotalFailed++;
                        continue;
                    }

                    // Create booking
                    var createDto = new CreateBookingDto
                    {
                        CourtId = dto.CourtId,
                        StartTime = startDateTime,
                        EndTime = endDateTime,
                        Note = dto.Note
                    };

                    var booking = await CreateBookingAsync(userId, createDto);
                    
                    // Mark as part of recurring group
                    var bookingEntity = await _unitOfWork.Bookings.GetByIdAsync(booking.Id);
                    if (bookingEntity != null)
                    {
                        bookingEntity.RecurringGroupId = recurringGroupId;
                        _unitOfWork.Bookings.Update(bookingEntity);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    result.SuccessfulBookings.Add(booking);
                    result.TotalSuccess++;
                    remainingBalance -= totalPrice;
                }
                catch (Exception ex)
                {
                    result.Conflicts.Add(new ConflictDto
                    {
                        Date = date,
                        Reason = ex.Message
                    });
                    result.TotalFailed++;
                }
            }

            return result;
        }

        public async Task<bool> CancelBookingAsync(int bookingId, string userId)
        {
            var member = await _unitOfWork.Members.FirstOrDefaultAsync(m => m.UserId == userId);
            if (member == null)
                throw new Exception("Không tìm thấy hội viên");

            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
            if (booking == null)
                throw new Exception("Không tìm thấy booking");

            if (booking.MemberId != member.Id)
                throw new Exception("Không có quyền thao tác booking này");

            if (booking.Status == BookingStatus.Cancelled)
                throw new Exception("Booking đã được hủy");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Calculate refund (100% if > 24h before, 50% if > 6h, 0% otherwise)
                var hoursUntilBooking = (booking.StartTime - DateTime.Now).TotalHours;
                var refundRate = hoursUntilBooking > 24 ? 1.0m :
                                hoursUntilBooking > 6 ? 0.5m : 0m;

                var refundAmount = booking.TotalPrice * refundRate;

                // Refund to wallet
                if (refundAmount > 0)
                {
                    await _walletService.AddBalanceAsync(
                        member.Id,
                        refundAmount,
                        $"REFUND-{booking.Id}",
                        $"Hoàn tiền hủy đặt sân (Refund {refundRate * 100}%)");
                }

                booking.Status = BookingStatus.Cancelled;
                _unitOfWork.Bookings.Update(booking);

                await _unitOfWork.CommitTransactionAsync();

                // Invalidate cache
                await InvalidateBookingCache(booking.CourtId, booking.StartTime);

                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<List<BookingDto>> GetMyBookingsAsync(string userId)
        {
            var member = await _unitOfWork.Members.FirstOrDefaultAsync(m => m.UserId == userId);
            if (member == null)
                throw new Exception("Không tìm thấy hội viên");

            var bookings = await _unitOfWork.Bookings
                .FindAsync(b => b.MemberId == member.Id);

            var result = new List<BookingDto>();
            foreach (var booking in bookings.OrderByDescending(b => b.CreatedDate))
            {
                var court = await _unitOfWork.Courts.GetByIdAsync(booking.CourtId);
                result.Add(MapToDto(booking, court!.Name, member.FullName));
            }

            return result;
        }

        public async Task<List<BookingDto>> GetPendingPaymentBookingsAsync()
        {
            var bookings = await _unitOfWork.Bookings
                .FindAsync(b => b.Status == BookingStatus.PendingPayment);

            var result = new List<BookingDto>();
            foreach (var booking in bookings)
            {
                var court = await _unitOfWork.Courts.GetByIdAsync(booking.CourtId);
                var member = await _unitOfWork.Members.GetByIdAsync(booking.MemberId);
                result.Add(MapToDto(booking, court!.Name, member!.FullName));
            }

            return result;
        }

        private async Task<bool> CheckTimeConflictAsync(int courtId, DateTime startTime, DateTime endTime)
        {
            var conflicts = await _unitOfWork.Bookings.FindAsync(b =>
                b.CourtId == courtId &&
                b.Status != BookingStatus.Cancelled &&
                ((b.StartTime < endTime && b.EndTime > startTime)));

            return conflicts.Any();
        }

        private async Task<List<TimeSlot>> GenerateSlotsForDay(Court court, DateTime date)
        {
            var slots = new List<TimeSlot>();
            var startHour = 6; // 6 AM
            var endHour = 22; // 10 PM

            for (int hour = startHour; hour < endHour; hour++)
            {
                var slotStart = date.Date.AddHours(hour);
                var slotEnd = slotStart.AddHours(1);

                var isAvailable = !await CheckTimeConflictAsync(court.Id, slotStart, slotEnd);

                slots.Add(new TimeSlot
                {
                    StartTime = slotStart,
                    EndTime = slotEnd,
                    Price = court.HourlyRate,
                    IsAvailable = isAvailable
                });
            }

            return slots;
        }

        private List<DateTime> GenerateRecurringDates(DateTime startDate, DateTime endDate, List<DayOfWeek> daysOfWeek)
        {
            var dates = new List<DateTime>();
            var current = startDate.Date;

            while (current <= endDate.Date)
            {
                if (daysOfWeek.Contains(current.DayOfWeek))
                {
                    dates.Add(current);
                }
                current = current.AddDays(1);
            }

            return dates;
        }

        private async Task InvalidateBookingCache(int courtId, DateTime startTime)
        {
            var cacheKey = $"booking:slots:{startTime:yyyy-MM-dd}:{courtId}";
            await _cacheService.RemoveAsync(cacheKey);
            
            var allCourtsKey = $"booking:slots:{startTime:yyyy-MM-dd}:";
            await _cacheService.RemoveAsync(allCourtsKey);
        }

        private BookingDto MapToDto(Booking booking, string courtName, string memberName)
        {
            return new BookingDto
            {
                Id = booking.Id,
                CourtId = booking.CourtId,
                CourtName = courtName,
                MemberId = booking.MemberId,
                MemberName = memberName,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status.ToString(),
                Note = booking.Note,
                CreatedDate = booking.CreatedDate,
                RecurringGroupId = booking.RecurringGroupId
            };
        }
    }
}
