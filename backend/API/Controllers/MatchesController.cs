using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PCM.API.Hubs;
using PCM.Application.DTOs.Matches;
using PCM.Application.Interfaces;

namespace PCM.API.Controllers
{
    [ApiController]
    [Route("api/matches")]
    [Authorize]
    public class MatchesController : ControllerBase
    {
        private readonly IMatchService _matchService;
        private readonly IHubContext<MatchHub> _matchHub;

        public MatchesController(IMatchService matchService, IHubContext<MatchHub> matchHub)
        {
            _matchService = matchService;
            _matchHub = matchHub;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? tournamentId)
        {
            var data = await _matchService.GetMatchesAsync(tournamentId);
            return Ok(data);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _matchService.GetByIdAsync(id);
            if (data == null)
                return NotFound();

            return Ok(data);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> Create([FromBody] CreateMatchDto dto)
        {
            var created = await _matchService.CreateMatchAsync(dto);
            await _matchHub.Clients.All.SendAsync("matchCreated", created);
            return Ok(created);
        }

        [HttpPut("{id:int}/result")]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] UpdateMatchResultDto dto)
        {
            dto.MatchId = id;
            var updated = await _matchService.UpdateMatchResultAsync(dto);
            await _matchHub.Clients.All.SendAsync("matchUpdated", updated);
            return Ok(updated);
        }
    }
}
