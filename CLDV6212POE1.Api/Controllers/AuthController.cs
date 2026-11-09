using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CLDV6212POE1.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthTestController : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        return Ok(new
        {
            message = "JWT authentication successful.",
            name = User.Identity?.Name,
            claims = User.Claims.Select(claim => new
            {
                claim.Type,
                claim.Value
            })
        });
    }
}