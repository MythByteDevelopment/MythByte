using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MythByte.Shared.Responses;
using MythByte.Shared.SystemInfo;
using MythByte.Shared.Versioning;

namespace MythByte.API.Controllers;

[ApiController]
[Route("api/system")]
public class SystemController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("version")]
    public ActionResult<ApiResponse<VersionResponse>> GetVersion()
    {
        var version = AppVersion.Of(typeof(SystemController).Assembly);

        return Ok(ApiResponse<VersionResponse>.Ok(new VersionResponse { Version = version }));
    }
}
