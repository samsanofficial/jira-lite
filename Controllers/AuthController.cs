using jira_lite.DTOs;
using jira_lite.Services;
using Microsoft.AspNetCore.Mvc;

namespace jira_lite.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, message, user) = await _authService.RegisterAsync(request);

        if (!success)
            return Conflict(new { message });

        return Ok(new { message, user });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, message, response) = await _authService.LoginAsync(request);

        if (!success)
            return Unauthorized(new { message });

        return Ok(response);
    }
}
