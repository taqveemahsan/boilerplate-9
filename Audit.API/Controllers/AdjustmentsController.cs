using System.Collections.Generic;
using System.Linq;
using Audit.Services;
using Audit.Services.Interfaces;
using AuthPilot.Models.Adjustments;
using AuthPilot.Models.Common;
using AuthPilot.Models.FiscalPeriods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditPilot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdjustmentsController : ControllerBase
    {
        private readonly IAdjustmentService _service;

        public AdjustmentsController(IAdjustmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<AdjustmentEntryDto>>>> List([FromQuery] AdjustmentEntryQuery query, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Fail<PagedResult<AdjustmentEntryDto>>(CollectModelErrors()));
            }

            try
            {
                var result = await _service.ListAsync(query, ct);
                Response.Headers["X-Total-Count"] = result.Total.ToString();
                return Ok(ApiResponse.Ok(result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse.Fail<PagedResult<AdjustmentEntryDto>>(ex.Message));
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<AdjustmentEntryDto>>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var entry = await _service.GetAsync(id, ct);
            if (entry == null)
            {
                return NotFound(ApiResponse.Fail<AdjustmentEntryDto>("Adjustment entry not found"));
            }

            return Ok(ApiResponse.Ok(entry));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<AdjustmentEntryDto>>> Create([FromBody] AdjustmentEntryCreateRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Fail<AdjustmentEntryDto>(CollectModelErrors()));
            }

            try
            {
                var created = await _service.CreateAsync(request, ct);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse.Ok(created));
            }
            catch (AdjustmentValidationException ex)
            {
                return BadRequest(ApiResponse.Fail<AdjustmentEntryDto>(ex.Errors));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse.Fail<AdjustmentEntryDto>(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse.Fail<AdjustmentEntryDto>(ex.Message));
            }
        }

        [HttpPost("update/{id:int}")]
        public async Task<ActionResult<ApiResponse<AdjustmentEntryDto>>> Update([FromRoute] int id, [FromBody] AdjustmentEntryUpdateRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Fail<AdjustmentEntryDto>(CollectModelErrors()));
            }

            try
            {
                var updated = await _service.UpdateAsync(id, request, ct);
                return Ok(ApiResponse.Ok(updated));
            }
            catch (AdjustmentValidationException ex)
            {
                return BadRequest(ApiResponse.Fail<AdjustmentEntryDto>(ex.Errors));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse.Fail<AdjustmentEntryDto>("Adjustment entry not found"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse.Fail<AdjustmentEntryDto>(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse.Fail<AdjustmentEntryDto>(ex.Message));
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
                return NotFound(ApiResponse.Fail<object?>("Adjustment entry not found"));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse.Fail<object?>(ex.Message));
            }
        }

        private IEnumerable<string> CollectModelErrors()
        {
            return ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Validation error" : e.ErrorMessage);
        }
    }
}
