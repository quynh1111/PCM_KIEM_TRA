using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PCM.Application.DTOs.Treasury;
using PCM.Application.Interfaces;

namespace PCM.API.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ITreasuryService _treasury;

        public TransactionsController(ITreasuryService treasury)
        {
            _treasury = treasury;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var data = await _treasury.GetTransactionsAsync(ct);
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TreasuryTransactionCreateDto dto, CancellationToken ct)
        {
            var createdByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(createdByUserId))
                return Unauthorized();

            var created = await _treasury.CreateTransactionAsync(createdByUserId, dto, ct);
            return Ok(created);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> Summary(CancellationToken ct)
        {
            var sum = await _treasury.GetSummaryAsync(ct);
            return Ok(sum);
        }
    }
}
