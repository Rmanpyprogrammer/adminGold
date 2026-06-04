using Core.API.Services.Digikala;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class DigikalaController : ControllerBase
{
    private readonly DigikalaSyncService
        _syncService;   

    public DigikalaController(
        DigikalaSyncService syncService
    )
    {
        _syncService = syncService;
    }

    [HttpGet("sync/{packageId:long}")]
    public async Task<IActionResult>
        SyncPackage(long packageId)
    {
        try
        {
            var result =
                await _syncService
                    .SyncPackageAsync(
                        packageId
                    );

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(
                new
                {
                    error = ex.Message
                }
            );
        }
    }
}