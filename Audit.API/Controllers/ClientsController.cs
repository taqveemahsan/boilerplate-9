using Audit.Services.Interfaces;
using AuthPilot.Models.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditPilot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientDto>>> GetAll()
        {
            var list = await _clientService.GetAllAsync();
            return Ok(list);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ClientDto>> GetById(Guid id)
        {
            var item = await _clientService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<ClientDto>> Create([FromBody] CreateClientModel model)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var created = await _clientService.CreateAsync(model);
            if (created == null) return BadRequest();
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClientModel model)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var ok = await _clientService.UpdateAsync(id, model);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _clientService.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}

