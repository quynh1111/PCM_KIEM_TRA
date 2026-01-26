using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PCM.Application.DTOs.Tournaments;
using PCM.Application.Interfaces;

namespace PCM.API.Controllers
{
    [ApiController]
    [Route("api/tournaments")]
    [Authorize]
    public class TournamentsController : ControllerBase
    {
        private readonly ITournamentService _tournamentService;

        public TournamentsController(ITournamentService tournamentService)
        {
            _tournamentService = tournamentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _tournamentService.GetAllTournamentsAsync();
            return Ok(data);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _tournamentService.GetByIdAsync(id);
            if (data == null)
                return NotFound();

            return Ok(data);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> Create([FromBody] CreateTournamentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var created = await _tournamentService.CreateTournamentAsync(dto, userId);
            return Ok(created);
        }

        [HttpPost("{id:int}/join")]
        public async Task<IActionResult> Join(int id, [FromBody] JoinTournamentDto? dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            try
            {
                var ok = await _tournamentService.JoinTournamentAsync(id, userId, dto?.TeamName);
                return Ok(new { joined = ok });
            }
            catch (Exception ex)
            {
                return MapTournamentError(ex);
            }
        }

        [HttpGet("{id:int}/bracket")]
        public async Task<IActionResult> Bracket(int id)
        {
            var data = await _tournamentService.GetBracketAsync(id);
            if (data == null)
                return NotFound();

            return Ok(data);
        }

        [HttpPost("{id:int}/generate-bracket")]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> GenerateBracket(int id)
        {
            try
            {
                var ok = await _tournamentService.GenerateBracketAsync(id);
                if (!ok)
                    return BadRequest(new { message = "Not enough participants to generate bracket" });

                return Ok(new { generated = ok });
            }
            catch (Exception ex)
            {
                return MapTournamentError(ex);
            }
        }

        private IActionResult MapTournamentError(Exception ex)
        {
            var message = ex.Message ?? "Tournament error";
            if (message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("không tìm thấy", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message });

            if (message.Contains("already", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("đã đăng ký", StringComparison.OrdinalIgnoreCase))
                return Conflict(new { message });

            if (message.Contains("full", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("closed", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("not open", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("chưa mở", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("hết hạn", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message });

            if (message.Contains("insufficient", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("không đủ", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message });

            return BadRequest(new { message });
        }
    }
}
