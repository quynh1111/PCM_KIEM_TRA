using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PCM.Application.DTOs.Wallet;
using PCM.Application.Interfaces;

namespace PCM.API.Controllers
{
    [ApiController]
    [Route("api/wallet")]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> CreateDeposit([FromBody] WalletDepositRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var created = await _walletService.CreateDepositRequestAsync(userId, dto);
            return Ok(created);
        }

        [HttpPost("approve-deposit")]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> ApproveDeposit([FromBody] ApproveDepositDto dto)
        {
            var result = await _walletService.ApproveDepositAsync(dto.TransactionId, dto.Approved, dto.Note);
            return Ok(result);
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> Transactions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var data = await _walletService.GetTransactionHistoryAsync(userId);
            return Ok(data);
        }

        [HttpGet("balance")]
        public async Task<IActionResult> Balance()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var balance = await _walletService.GetBalanceAsync(userId);
            return Ok(new { balance });
        }
    }
}
