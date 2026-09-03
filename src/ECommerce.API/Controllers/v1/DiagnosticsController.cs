using System.Diagnostics;
using System.Runtime.InteropServices;
using Asp.Versioning;
using ECommerce.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/diagnostics")]
public class DiagnosticsController : BaseApiController
{
    private readonly IWebHostEnvironment _env;

    public DiagnosticsController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet("ping")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public IActionResult Ping()
    {
        return Ok(ApiResponse<string>.SuccessResponse("pong", $"Server healthy at {DateTime.UtcNow:O}"));
    }

    [HttpGet("info")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult GetSystemInfo()
    {
        var process = Process.GetCurrentProcess();
        var memoryMb = Math.Round((double)process.WorkingSet64 / (1024 * 1024), 2);

        var info = new
        {
            Application = "Global E-Commerce Platform API",
            Version = "1.0.0",
            Environment = _env.EnvironmentName,
            Framework = RuntimeInformation.FrameworkDescription,
            OS = RuntimeInformation.OSDescription,
            MemoryWorkingSetMb = memoryMb,
            ServerTimeUtc = DateTime.UtcNow
        };

        return Ok(ApiResponse<object>.SuccessResponse(info));
    }
}
