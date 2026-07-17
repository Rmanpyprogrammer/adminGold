using System.Text.Json;
using Core.API.DTOs;
using Core.API.Services.Digikala;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace Core.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class DigikalaController : ControllerBase
{
    private readonly DigikalaSyncService _syncService;   
    private readonly AdminService _adminService;   

    public DigikalaController(
        DigikalaSyncService syncService,
        AdminService adminService

    )
    {
        _syncService = syncService;
        _adminService = adminService;
    }

    [HttpGet("sync/{packageId:long}/{adminid:long}")]
    // [Authorize]
    public async Task<IActionResult> 
    SyncPackage(long packageId,
                int page = 1,
                int adminid = 1)
    {
        try
        {
            var result =
                await _syncService
                    .SyncPackageAsync(
                        packageId,
                        page,
                        adminid
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


    [HttpPost("reconciliation/")]
    // [Authorize]
    public async Task<IActionResult> 
    Reconciliation(
        [FromBody] ReconciliationRequestDto request
    )
    {
        
        try
        {
            var result =
                await _syncService
                    .ReconciliationService(
                        request.Start,
                        request.End,
                        request.AdminId
                        
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

    [HttpPost("report/")]
    // [Authorize]
    public async Task<IActionResult> 
    Report(
        [FromBody] AdminReportDtoRequest request
    )
    {
        
        try
        {
            var result =
                await _adminService
                    .CalculateReport(
                        request.Start,
                        request.End,
                        request.AdminId
                        
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