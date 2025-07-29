using Audit.Services.Interfaces;
using AuthPilot.Models.Auth;
using Microsoft.AspNetCore.Mvc;

namespace AuditPilot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] RegisterModel model)
        {
            var result = await _userService.RegisterAsync(model);
            if (!result) return BadRequest("Registration failed");
            return Ok("Registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var token = await _userService.LoginAsync(model);
            if (token == null) return Unauthorized();
            return Ok(new { token });
        }
    }
}
