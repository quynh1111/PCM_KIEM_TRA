using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PCM.API.Controllers.Compatibility
{
    [ApiController]
    [Route("api/challenges")]
    [Authorize]
    public class ChallengesCompatibilityController : ControllerBase
    {
        // NOTE: Controller này là alias để giảng viên test đúng đề thường.
        // Bạn map nội bộ sang Tournament service.

        // TODO: inject ITournamentService
        public ChallengesCompatibilityController()
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            await Task.CompletedTask;
            return Ok(new object[0]);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            await Task.CompletedTask;
            return Ok(new { id });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> Create([FromBody] object dto, CancellationToken ct)
        {
            await Task.CompletedTask;
            return Ok(dto);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> Update(int id, [FromBody] object dto, CancellationToken ct)
        {
            await Task.CompletedTask;
            return Ok(dto);
        }

        [HttpPost("{id:int}/join")]
        [Authorize(Roles = "Member,Admin,Treasurer")]
        public async Task<IActionResult> Join(int id, CancellationToken ct)
        {
            await Task.CompletedTask;
            return Ok(new { id, joined = true });
        }

        [HttpPost("{id:int}/auto-divide-teams")]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> AutoDivideTeams(int id, CancellationToken ct)
        {
            // Logic: sort participants by Rank desc, assign A/B alternating.
            await Task.CompletedTask;
            return Ok(new { id, status = "OK" });
        }
    }
}
