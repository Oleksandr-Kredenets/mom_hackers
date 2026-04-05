using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.Application.DTOs;
using Web.Helpers;

namespace Web.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class MeController : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType(typeof(AuthUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        if (!AuthClaims.TryGetAuthUser(User, out var user) || user is null)
            return Unauthorized();

        return Ok(user);
    }
}
