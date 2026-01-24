using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PCM.Application.DTOs.Treasury;
using PCM.Application.Interfaces;
using PCM.Domain.Enums;

namespace PCM.API.Controllers
{
    [ApiController]
    [Route("api/transaction-categories")]
    [Authorize(Roles = "Admin,Treasurer")]
    public class TransactionCategoriesController : ControllerBase
    {
        private readonly ITransactionCategoryService _svc;

        public TransactionCategoriesController(ITransactionCategoryService svc)
        {
            _svc = svc;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] TransactionScope? scope, CancellationToken ct)
        {
            var data = await _svc.GetAsync(scope, ct);
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TransactionCategoryCreateDto dto, CancellationToken ct)
        {
            var created = await _svc.CreateAsync(dto, ct);
            return Ok(created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TransactionCategoryCreateDto dto, CancellationToken ct)
        {
            var updated = await _svc.UpdateAsync(id, dto, ct);
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _svc.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
