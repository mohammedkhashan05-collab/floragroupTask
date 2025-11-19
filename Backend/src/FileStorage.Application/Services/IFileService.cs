using FileStorage.Application.DTOs;

namespace FileStorage.Application.Services;

public interface IFileService
{
    Task<FileResponseDto> UploadFileAsync(FileUploadDto uploadDto, Guid userId, CancellationToken cancellationToken = default);
    Task<PagedResultDto<FileResponseDto>> GetFilesAsync(int pageNumber, int pageSize, string? searchTerm, CancellationToken cancellationToken = default);
    Task<FileResponseDto?> GetFileByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Stream> DownloadFileAsync(Guid id, CancellationToken cancellationToken = default);
    Task SoftDeleteFileAsync(Guid id, CancellationToken cancellationToken = default);
    Task HardDeleteFileAsync(Guid id, CancellationToken cancellationToken = default);
}

