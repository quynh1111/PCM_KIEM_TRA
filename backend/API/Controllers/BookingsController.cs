using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
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

            var created = await _bookingService.CreateBookingAsync(userId, dto);
            return Ok(created);
        }

        [HttpPost("recurring")]
        public async Task<IActionResult> CreateRecurring([FromBody] CreateRecurringBookingDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var result = await _bookingService.CreateRecurringBookingAsync(userId, dto);
            return Ok(result);
        }

        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var ok = await _bookingService.CancelBookingAsync(id, userId);
            return Ok(new { cancelled = ok });
        }

        [HttpGet("me")]
        public async Task<IActionResult> MyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var data = await _bookingService.GetMyBookingsAsync(userId);
            return Ok(data);
        }
    }
}
