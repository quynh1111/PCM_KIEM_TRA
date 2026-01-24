using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PCM.Application.DTOs.News;
using PCM.Application.Interfaces;

namespace PCM.API.Controllers
{
    [ApiController]
    [Route("api/news")]
    [Authorize]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsService;

        public NewsController(INewsService newsService)
        {
            _newsService = newsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool onlyPinned = false)
        {
            var data = await _newsService.GetAllAsync(onlyPinned);
            return Ok(data);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _newsService.GetByIdAsync(id);
            if (data == null)
                return NotFound();

            return Ok(data);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> Create([FromBody] CreateNewsDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var created = await _newsService.CreateAsync(dto, userId);
            return Ok(created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateNewsDto dto)
        {
            var updated = await _newsService.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _newsService.DeleteAsync(id);
            if (!ok)
                return NotFound();

            return NoContent();
        }
    }
}
