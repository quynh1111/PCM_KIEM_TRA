using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PCM.Application.DTOs.Bookings;

namespace PCM.Application.Interfaces
{
    public interface IBookingService
    {
        Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(DateTime date, int? courtId = null);
        
        /// <summary>
        /// CRITICAL: Uses optimistic locking to prevent race conditions
        /// </summary>
        Task<BookingDto> CreateBookingAsync(string userId, CreateBookingDto dto);
        
        /// <summary>
        /// Create recurring bookings with conflict detection
        /// </summary>
        Task<RecurringBookingResultDto> CreateRecurringBookingAsync(string userId, CreateRecurringBookingDto dto);
        
        Task<bool> CancelBookingAsync(int bookingId, string userId);
        Task<List<BookingDto>> GetMyBookingsAsync(string userId);
        Task<List<BookingDto>> GetPendingPaymentBookingsAsync();
    }
}
