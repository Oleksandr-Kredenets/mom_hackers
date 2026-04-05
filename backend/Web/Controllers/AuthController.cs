using Microsoft.AspNetCore.Mvc;
using TMS.Application.Interfaces;
using TMS.Domain.Models;

namespace Web.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest body)
    {
        try
        {
            await _userService.RegisterAsync(body.Name, body.Email, body.Password);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest body)
    {
        try
        {
            var auth = await _userService.LoginAsync(body.Email, body.Password);
            return Ok(auth);
        }
        catch (InvalidOperationException)
        {
            return Unauthorized();
        }
    }
}
