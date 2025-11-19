using FileStorage.Application.DTOs;
using FileStorage.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FileStorage.API.Controllers;

/// <summary>
/// Controller for file operations.
/// Handles upload, download, list, preview, and delete operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly IValidator<FileUploadDto> _validator;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        IFileService fileService,
        IValidator<FileUploadDto> validator,
        ILogger<FilesController> logger)
    {
        _fileService = fileService;
        _validator = validator;
        _logger = logger;
    }


    [HttpPost]
    public async Task<ActionResult<FileResponseDto>> UploadFile([FromForm] FileUploadDto uploadDto, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(uploadDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _fileService.UploadFileAsync(uploadDto, userId, cancellationToken);
        
        return CreatedAtAction(nameof(GetFileById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<FileResponseDto>>> GetFiles(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _fileService.GetFilesAsync(pageNumber, pageSize, searchTerm, cancellationToken);
        return Ok(result);
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<FileResponseDto>> GetFileById(Guid id, CancellationToken cancellationToken)
    {
        var file = await _fileService.GetFileByIdAsync(id, cancellationToken);
        if (file == null)
        {
            return NotFound();
        }

        return Ok(file);
    }

   
    [HttpGet("{id}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadFile(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var file = await _fileService.GetFileByIdAsync(id, cancellationToken);
            if (file == null)
            {
                _logger.LogWarning("File not found for download: {FileId}", id);
                return NotFound();
            }

            var stream = await _fileService.DownloadFileAsync(id, cancellationToken);
            
            _logger.LogInformation("Serving download for file: {FileId}, Name: {FileName}, Type: {ContentType}, Size: {Size}", 
                id, file.OriginalFileName, file.ContentType, file.SizeInBytes);
            
            return new FileStreamResult(stream, file.ContentType)
            {
                FileDownloadName = file.OriginalFileName,
                EnableRangeProcessing = true
            };
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "File not found for download: {FileId}", id);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serving download for file: {FileId}", id);
            throw;
        }
    }

    [HttpGet("{id}/preview")]
    public async Task<IActionResult> PreviewFile(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var file = await _fileService.GetFileByIdAsync(id, cancellationToken);
            if (file == null)
            {
                _logger.LogWarning("File not found for preview: {FileId}", id);
                return NotFound();
            }

            var previewableTypes = new[] { "image/", "application/pdf" };
            if (!previewableTypes.Any(t => file.ContentType.StartsWith(t, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("File type not previewable: {ContentType}", file.ContentType);
                return BadRequest("File type does not support preview");
            }

            var stream = await _fileService.DownloadFileAsync(id, cancellationToken);
            
            _logger.LogInformation("Serving preview for file: {FileId}, Type: {ContentType}, Size: {Size}", 
                id, file.ContentType, file.SizeInBytes);
            
            return new FileStreamResult(stream, file.ContentType)
            {
                EnableRangeProcessing = true
            };
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "File not found for preview: {FileId}", id);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serving preview for file: {FileId}", id);
            throw;
        }
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDeleteFile(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _fileService.SoftDeleteFileAsync(id, cancellationToken);
            return NoContent();
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }


    [HttpDelete("{id}/hard")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> HardDeleteFile(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _fileService.HardDeleteFileAsync(id, cancellationToken);
            return NoContent();
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }
}

