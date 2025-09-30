using Audit.Services.Interfaces;
using AuthPilot.Models.Common;
using AuthPilot.Models.FiscalPeriods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditPilot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FiscalPeriodsController : ControllerBase
    {
        private readonly IFiscalPeriodService _service;

        public FiscalPeriodsController(IFiscalPeriodService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<FiscalPeriodDto>>>> List([FromQuery] FiscalPeriodQuery query, CancellationToken ct)
        {
            var result = await _service.ListAsync(query, ct);
            Response.Headers["X-Total-Count"] = result.Total.ToString();
            return Ok(ApiResponse.Ok(result));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<FiscalPeriodDto>>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var period = await _service.GetAsync(id, ct);
            if (period == null)
            {
                return NotFound(ApiResponse.Fail<FiscalPeriodDto>("Fiscal period not found"));
            }

            return Ok(ApiResponse.Ok(period));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<FiscalPeriodDto>>> Create([FromBody] FiscalPeriodCreateRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Fail<FiscalPeriodDto>(ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Validation error" : e.ErrorMessage)));
            }

            try
            {
                var created = await _service.CreateAsync(request, ct);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse.Ok(created));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse.Fail<FiscalPeriodDto>(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse.Fail<FiscalPeriodDto>(ex.Message));
            }
        }

        [HttpPost("update/{id:int}")]
        public async Task<ActionResult<ApiResponse<FiscalPeriodDto>>> Update([FromRoute] int id, [FromBody] FiscalPeriodUpdateRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Fail<FiscalPeriodDto>(ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Validation error" : e.ErrorMessage)));
            }

            try
            {
                var updated = await _service.UpdateAsync(id, request, ct);
                return Ok(ApiResponse.Ok(updated));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse.Fail<FiscalPeriodDto>("Fiscal period not found"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse.Fail<FiscalPeriodDto>(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse.Fail<FiscalPeriodDto>(ex.Message));
            }
        }

        [HttpPost("delete/{id:int}")]
        public async Task<ActionResult<ApiResponse<object?>>> Delete([FromRoute] int id, CancellationToken ct)
        {
            try
            {
                await _service.DeleteAsync(id, ct);
                return Ok(ApiResponse.Ok<object?>(null));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse.Fail<object?>("Fiscal period not found"));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse.Fail<object?>(ex.Message));
            }
        }

        [HttpPost("lock/{id:int}")]
        public async Task<ActionResult<ApiResponse<FiscalPeriodDto>>> Lock([FromRoute] int id, CancellationToken ct)
        {
            try
            {
                var locked = await _service.LockAsync(id, ct);
                return Ok(ApiResponse.Ok(locked));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse.Fail<FiscalPeriodDto>("Fiscal period not found"));
            }
        }
    }
}
