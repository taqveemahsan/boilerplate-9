using Audit.Services.Interfaces;
using AuthPilot.Models.Accounting;
using AuthPilot.Models.TrialBalanceRows;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuditPilot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TrialBalanceController : ControllerBase
    {
        private readonly ITrialBalanceService _service;

        public TrialBalanceController(ITrialBalanceService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<TrialBalanceDto>> Get([FromBody] TrialBalanceRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            try
            {
                var tb = await _service.GetAsync(request, ct);
                return Ok(tb);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // -------------------- Persisted Trial Balance Rows Endpoints --------------------

        [HttpGet]
        public async Task<ActionResult<AuthPilot.Models.TrialBalanceRows.PagedResult<AuthPilot.Models.TrialBalanceRows.TrialBalanceRowDto>>> List([FromQuery] AuthPilot.Models.TrialBalanceRows.TrialBalanceQuery q, CancellationToken ct)
        {
            try
            {
                var result = await _service.ListAsync(q, ct);
                Response.Headers["X-Total-Count"] = result.Total.ToString();
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AuthPilot.Models.TrialBalanceRows.TrialBalanceRowDto>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var item = await _service.GetAsync(id, ct);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] AuthPilot.Models.TrialBalanceRows.TrialBalanceRowUpdateDto dto, CancellationToken ct)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dto, ct);
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "Concurrency conflict" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            try
            {
                await _service.DeleteAsync(id, ct);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("import")]
        [RequestSizeLimit(26 * 1024 * 1024)]
        public async Task<ActionResult<AuthPilot.Models.TrialBalanceRows.TrialBalanceImportResponse>> Import([FromForm] IFormFile file, [FromForm] int fiscalPeriodId, [FromForm] bool force = false, CancellationToken ct = default)
        {
            if (file == null) return BadRequest(new { message = "file is required" });
            try
            {
                await using var s = file.OpenReadStream();
                var resp = await _service.ImportAsync(fiscalPeriodId, s, file.FileName, force, ct);
                return Accepted(resp);
            }
            catch (NotSupportedException ex)
            {
                return StatusCode(StatusCodes.Status415UnsupportedMediaType, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] int fiscalPeriodId, [FromQuery] string? format, CancellationToken ct)
        {
            try
            {
                var (content, fileName, contentType) = await _service.ExportAsync(fiscalPeriodId, format ?? "csv", ct);
                return File(content, contentType, fileName);
            }
            catch (NotSupportedException ex)
            {
                return StatusCode(StatusCodes.Status415UnsupportedMediaType, new { message = ex.Message });
            }
        }

        [HttpGet("summary")]
        public async Task<ActionResult<AuthPilot.Models.TrialBalanceRows.TrialBalanceSummaryDto>> Summary([FromQuery] int fiscalPeriodId, CancellationToken ct)
        {
            var s = await _service.SummaryAsync(fiscalPeriodId, ct);
            return Ok(s);
        }
    }
}
