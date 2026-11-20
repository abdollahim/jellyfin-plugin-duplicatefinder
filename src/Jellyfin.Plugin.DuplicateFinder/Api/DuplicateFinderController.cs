using Jellyfin.Plugin.DuplicateFinder.Model;
using Jellyfin.Plugin.DuplicateFinder.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.DuplicateFinder.Api;

[ApiController]
[Route("DuplicateFinder")]
[Authorize(Policy = "RequiresElevation")]
public class DuplicateFinderController : ControllerBase
{
    private readonly DuplicateScanService _service;

    public DuplicateFinderController(DuplicateScanService service) => _service = service;

    [HttpGet("Stats")]
    public ActionResult<ScanStats> Stats() => _service.GetStats();

    [HttpPost("Start")]
    public IActionResult Start()
    {
        _service.StartScan();
        return Accepted();
    }

    [HttpGet("Status")]
    public ActionResult<ScanJob> Status() => _service.Job;

    [HttpPost("Verify")]
    public IActionResult Verify([FromQuery] string groupId)
    {
        return _service.StartVerify(groupId) ? Accepted() : NotFound();
    }
}
