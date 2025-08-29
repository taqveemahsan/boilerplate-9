using Audit.Services.Interfaces;
using AuthPilot.Models.ShareHolders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditPilot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShareHoldersController : ControllerBase
    {
        private readonly IShareHolderService _service;

        public ShareHoldersController(IShareHolderService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShareHolderDto>>> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<ActionResult<ShareHolderDto>> Create([FromBody] CreateShareHolderModel model)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var created = await _service.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = created!.Id }, created);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ShareHolderDto>> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateShareHolderModel model)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var ok = await _service.UpdateAsync(id, model);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
