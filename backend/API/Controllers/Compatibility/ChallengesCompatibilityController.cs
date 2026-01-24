using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PCM.Application.DTOs.Tournaments;
using PCM.Application.Interfaces;

namespace PCM.API.Controllers.Compatibility
{
    [ApiController]
    [Route("api/challenges")]
    [Authorize]
    public class ChallengesCompatibilityController : ControllerBase
    {
        // NOTE: Controller nay la alias de giang vien test dung de thuong.
        // Map noi bo sang Tournament service.
        private readonly ITournamentService _tournamentService;

        public ChallengesCompatibilityController(ITournamentService tournamentService)
        {
            _tournamentService = tournamentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var data = await _tournamentService.GetAllTournamentsAsync();
            return Ok(data);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var data = await _tournamentService.GetByIdAsync(id);
            if (data == null)
                return NotFound();

            return Ok(data);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> Create([FromBody] CreateTournamentDto dto, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var created = await _tournamentService.CreateTournamentAsync(dto, userId);
            return Ok(created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateTournamentDto dto, CancellationToken ct)
        {
            var updated = await _tournamentService.UpdateTournamentAsync(id, dto);
            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        [HttpPost("{id:int}/join")]
        [Authorize(Roles = "Member,Admin,Treasurer")]
        public async Task<IActionResult> Join(int id, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var ok = await _tournamentService.JoinTournamentAsync(id, userId, null);
            return Ok(new { id, joined = ok });
        }

        [HttpPost("{id:int}/auto-divide-teams")]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> AutoDivideTeams(int id, CancellationToken ct)
        {
            // Logic: sort participants by Rank desc, assign A/B alternating.
            var ok = await _tournamentService.AutoDivideTeamsAsync(id);
            return Ok(new { id, status = ok ? "OK" : "NoParticipants" });
        }
    }
}
