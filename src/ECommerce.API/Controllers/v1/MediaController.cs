using Asp.Versioning;
using ECommerce.Application.DTOs.Media;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/media")]
public class MediaController : BaseApiController
{
    private readonly IFileStorageService _storageService;
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public MediaController(IFileStorageService storageService)
    {
        _storageService = storageService;
    }

    [HttpPost("upload")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<FileUploadResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadImage(IFormFile? file, [FromQuery] string folder = "products")
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<object>.FailureResponse("No file was uploaded."));
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return BadRequest(ApiResponse<object>.FailureResponse("File exceeds the maximum allowed size of 5MB."));
        }

        if (!_storageService.IsValidImageFile(file.FileName, file.ContentType))
        {
            return BadRequest(ApiResponse<object>.FailureResponse("Only supported image formats (.jpg, .jpeg, .png, .webp, .gif, .svg, .avif) are allowed."));
        }

        using var stream = file.OpenReadStream();
        var result = await _storageService.UploadAsync(
            stream,
            file.FileName,
            file.ContentType,
            folder,
            HttpContext.RequestAborted);

        return Ok(ApiResponse<FileUploadResultDto>.SuccessResponse(result, "Image uploaded successfully."));
    }

    [HttpDelete]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteImage([FromQuery] string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return BadRequest(ApiResponse<object>.FailureResponse("A file path must be provided."));
        }

        var deleted = await _storageService.DeleteAsync(path, HttpContext.RequestAborted);
        if (!deleted)
        {
            return NotFound(ApiResponse<bool>.FailureResponse("File not found or could not be removed."));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Media asset successfully removed."));
    }
}
