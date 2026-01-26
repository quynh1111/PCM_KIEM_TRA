using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PCM.Application.DTOs.Members;
using PCM.Application.Interfaces;

namespace PCM.API.Controllers
{
    [ApiController]
    [Route("api/members")]
    [Authorize]
    public class MembersController : ControllerBase
    {
        private readonly IMemberService _memberService;

        public MembersController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var member = await _memberService.GetByUserIdAsync(userId);
            if (member == null)
                return NotFound();

            return Ok(member);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _memberService.GetAllAsync();
            return Ok(data);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _memberService.GetByIdAsync(id);
            if (data == null)
                return NotFound();

            return Ok(data);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateMemberProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var updated = await _memberService.UpdateProfileAsync(userId, dto);
            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMemberProfileDto dto)
        {
            var updated = await _memberService.UpdateMemberAsync(id, dto);
            if (updated == null)
                return NotFound();

            return Ok(updated);
        }
    }
}
