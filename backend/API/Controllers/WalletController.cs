using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PCM.API.Hubs;
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
        private readonly IHubContext<NotificationHub> _notificationHub;

        public WalletController(IWalletService walletService, IHubContext<NotificationHub> notificationHub)
        {
            _walletService = walletService;
            _notificationHub = notificationHub;
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> CreateDeposit([FromBody] WalletDepositRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            try
            {
                var created = await _walletService.CreateDepositRequestAsync(userId, dto);
                await _notificationHub.Clients.User(userId).SendAsync("walletDepositRequested", created);
                return Ok(created);
            }
            catch (Exception ex)
            {
                return MapWalletError(ex);
            }
        }

        [HttpPost("approve-deposit")]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> ApproveDeposit([FromBody] ApproveDepositDto dto)
        {
            try
            {
                var result = await _walletService.ApproveDepositAsync(dto.TransactionId, dto.Approved, dto.Note);
                await _notificationHub.Clients.All.SendAsync("walletDepositApproved", result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return MapWalletError(ex);
            }
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> PendingDeposits()
        {
            var data = await _walletService.GetPendingDepositsAsync();
            return Ok(data);
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> Transactions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            try
            {
                var data = await _walletService.GetTransactionHistoryAsync(userId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return MapWalletError(ex);
            }
        }

        [HttpGet("balance")]
        public async Task<IActionResult> Balance()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            try
            {
                var balance = await _walletService.GetBalanceAsync(userId);
                return Ok(new { balance });
            }
            catch (Exception ex)
            {
                return MapWalletError(ex);
            }
        }

        private IActionResult MapWalletError(Exception ex)
        {
            var message = ex.Message ?? "Wallet error";
            if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message });

            if (message.Contains("already processed", StringComparison.OrdinalIgnoreCase))
                return Conflict(new { message });

            return BadRequest(new { message });
        }
    }
}
