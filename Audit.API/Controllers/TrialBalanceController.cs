using Audit.Services.Interfaces;
using AuthPilot.Models.Accounting;
using AuthPilot.Models.TrialBalanceRows;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrialBalanceRowDto = AuthPilot.Models.TrialBalanceRows.TrialBalanceRowDto;

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


        [HttpPost("rows")]
        public async Task<ActionResult<TrialBalanceRowDto>> CreateRow([FromBody] TrialBalanceRowCreateDto dto, CancellationToken ct)
        {
            var res = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(Get), new { id = res.Id }, res);
        }

        // CREATE by AccountCode (optional)
        [HttpPost("rows/by-code")]
        public async Task<ActionResult<TrialBalanceRowDto>> CreateRowByCode([FromBody] TrialBalanceRowCreateByCodeDto dto, CancellationToken ct)
        {
            var res = await _service.CreateByCodeAsync(dto, ct);
            return CreatedAtAction(nameof(Get), new { id = res.Id }, res);
        }

        // Calculate/Fetch TB (already POST - keep as-is)
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
        public async Task<ActionResult<PagedResult<TrialBalanceRowDto>>> List([FromQuery] TrialBalanceQuery q, CancellationToken ct)
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
        public async Task<ActionResult<TrialBalanceRowDto>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var item = await _service.GetAsync(id, ct);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // Instead of PUT → expose as POST api/trialbalance/update/{id}
        [HttpPost("update/{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] TrialBalanceRowUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
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

        // Instead of DELETE → expose as POST api/trialbalance/delete/{id}
        [HttpPost("delete/{id:int}")]
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
        public async Task<ActionResult<TrialBalanceImportResponse>> Import([FromForm] IFormFile file, [FromForm] int fiscalPeriodId, [FromForm] bool force = false, CancellationToken ct = default)
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
        public async Task<ActionResult<TrialBalanceSummaryDto>> Summary([FromQuery] int fiscalPeriodId, CancellationToken ct)
        {
            var s = await _service.SummaryAsync(fiscalPeriodId, ct);
            return Ok(s);
        }
    }
}
