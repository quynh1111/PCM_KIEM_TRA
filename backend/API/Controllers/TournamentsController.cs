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
        public async Task<IActionResult> Join(int id, [FromBody] JoinTournamentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var ok = await _tournamentService.JoinTournamentAsync(id, userId, dto?.TeamName);
            return Ok(new { joined = ok });
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
            var ok = await _tournamentService.GenerateBracketAsync(id);
            return Ok(new { generated = ok });
        }
    }
}
