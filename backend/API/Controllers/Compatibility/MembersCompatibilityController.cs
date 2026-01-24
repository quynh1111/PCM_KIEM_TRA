using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PCM.Application.Interfaces;
namespace PCM.API.Controllers.Compatibility
{
    [ApiController]
    [Route("api/members")]
    [Authorize]
    public class MembersCompatibilityController : ControllerBase
    {
        private readonly IMemberService _memberService;
        public MembersCompatibilityController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        // BASIC required: GET /api/members/top-ranking?limit=5
        [HttpGet("top-ranking")]
        public async Task<IActionResult> TopRanking([FromQuery] int limit = 5, CancellationToken ct = default)
        {
            var result = await _memberService.GetTopRankingAsync(limit);
            return Ok(result);
        }
    }
}
