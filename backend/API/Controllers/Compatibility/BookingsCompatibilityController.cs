using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PCM.Application.Interfaces;

namespace PCM.API.Controllers.Compatibility
{
    [ApiController]
    [Route("api/bookings")]
    [Authorize]
    public class BookingsCompatibilityController : ControllerBase
    {
        private readonly IBookingService _booking;

        public BookingsCompatibilityController(IBookingService booking)
        {
            _booking = booking;
        }

        // BASIC required:
        // GET /api/bookings/available-slots?courtId=&date=YYYY-MM-DD
        [HttpGet("available-slots")]
        public async Task<IActionResult> AvailableSlots([FromQuery] int courtId, [FromQuery] DateOnly date, CancellationToken ct)
        {
            var slots = await _booking.GetAvailableSlotsAsync(date.ToDateTime(TimeOnly.MinValue), courtId);
            return Ok(slots);
        }
    }
}
