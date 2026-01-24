using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PCM.API.Hubs;
using PCM.Application.DTOs.Bookings;
using PCM.Application.Interfaces;

namespace PCM.API.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IHubContext<BookingHub> _bookingHub;

        public BookingsController(IBookingService bookingService, IHubContext<BookingHub> bookingHub)
        {
            _bookingService = bookingService;
            _bookingHub = bookingHub;
        }

        [HttpGet("slots")]
        public async Task<IActionResult> GetSlots([FromQuery] DateOnly date, [FromQuery] int? courtId, CancellationToken ct)
        {
            var result = await _bookingService.GetAvailableSlotsAsync(date.ToDateTime(TimeOnly.MinValue), courtId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            try
            {
                var created = await _bookingService.CreateBookingAsync(userId, dto);
                await _bookingHub.Clients.All.SendAsync("bookingUpdated", new
                {
                    courtId = dto.CourtId,
                    date = dto.StartTime.Date
                });
                return Ok(created);
            }
            catch (Exception ex)
            {
                return MapBookingError(ex);
            }
        }

        [HttpPost("recurring")]
        public async Task<IActionResult> CreateRecurring([FromBody] CreateRecurringBookingDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            try
            {
                var result = await _bookingService.CreateRecurringBookingAsync(userId, dto);
                await _bookingHub.Clients.All.SendAsync("bookingUpdated", new
                {
                    courtId = dto.CourtId,
                    date = dto.StartDate.Date
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return MapBookingError(ex);
            }
        }

        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            try
            {
                var ok = await _bookingService.CancelBookingAsync(id, userId);
                await _bookingHub.Clients.All.SendAsync("bookingUpdated", new
                {
                    bookingId = id
                });
                return Ok(new { cancelled = ok });
            }
            catch (Exception ex)
            {
                return MapBookingError(ex);
            }
        }

        [HttpGet("me")]
        public async Task<IActionResult> MyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            try
            {
                var data = await _bookingService.GetMyBookingsAsync(userId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return MapBookingError(ex);
            }
        }

        private IActionResult MapBookingError(Exception ex)
        {
            var message = ex.Message ?? "Booking error";
            if (message.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message });

            if (message.Contains("already booked", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("conflict", StringComparison.OrdinalIgnoreCase))
                return Conflict(new { message });

            return BadRequest(new { message });
        }
    }
}
