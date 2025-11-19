using Microsoft.AspNetCore.Http;

namespace FileStorage.Application.DTOs;


public class FileUploadDto
{
    public IFormFile File { get; set; } = null!;
    public string? Tags { get; set; }
}

