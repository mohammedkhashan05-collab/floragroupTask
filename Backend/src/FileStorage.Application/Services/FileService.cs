using AutoMapper;
using FileStorage.Application.DTOs;
using FileStorage.Domain.Entities;
using FileStorage.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FileStorage.Application.Services;


public class FileService : IFileService
{
    private readonly IFileMetadataRepository _fileRepository;
    private readonly IFileStorageService _storageService;
    private readonly IMapper _mapper;
    private readonly ILogger<FileService> _logger;

    public FileService(
        IFileMetadataRepository fileRepository,
        IFileStorageService storageService,
        IMapper mapper,
        ILogger<FileService> logger)
    {
        _fileRepository = fileRepository;
        _storageService = storageService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FileResponseDto> UploadFileAsync(FileUploadDto uploadDto, Guid userId, CancellationToken cancellationToken = default)
    {
        var fileId = Guid.NewGuid();
        var file = uploadDto.File;

        string checksum;
        string storagePath;

        using (var fileStream = file.OpenReadStream())
        {
            checksum = await _storageService.ComputeChecksumAsync(fileStream);
        }

        using (var fileStream = file.OpenReadStream())
        {
            storagePath = await _storageService.SaveFileAsync(fileId, fileStream, cancellationToken);
        }

        var fileMetadata = new FileMetadata
        {
            Id = fileId,
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            SizeInBytes = file.Length,
            Checksum = checksum,
            StoragePath = storagePath,
            Tags = uploadDto.Tags,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _fileRepository.AddAsync(fileMetadata, cancellationToken);

        _logger.LogInformation("File uploaded successfully: {FileId}, {FileName}", fileId, file.FileName);

        var result = await _fileRepository.GetByIdAsync(fileId, cancellationToken: cancellationToken);
        return _mapper.Map<FileResponseDto>(result!);
    }

    public async Task<PagedResultDto<FileResponseDto>> GetFilesAsync(int pageNumber, int pageSize, string? searchTerm, CancellationToken cancellationToken = default)
    {
        var skip = (pageNumber - 1) * pageSize;

        IEnumerable<FileMetadata> files;
        int totalCount;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            files = await _fileRepository.SearchAsync(searchTerm, skip, pageSize, cancellationToken: cancellationToken);
            totalCount = await _fileRepository.CountAsync(cancellationToken: cancellationToken);
        }
        else
        {
            files = await _fileRepository.GetAllAsync(skip, pageSize, cancellationToken: cancellationToken);
            totalCount = await _fileRepository.CountAsync(cancellationToken: cancellationToken);
        }

        return new PagedResultDto<FileResponseDto>
        {
            Items = _mapper.Map<IEnumerable<FileResponseDto>>(files),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<FileResponseDto?> GetFileByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(id, cancellationToken: cancellationToken);
        return file == null ? null : _mapper.Map<FileResponseDto>(file);
    }

    public async Task<Stream> DownloadFileAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (file == null)
        {
            throw new FileNotFoundException($"File with id {id} not found");
        }

        if (file.IsDeleted)
        {
            throw new InvalidOperationException($"File with id {id} has been deleted");
        }

        return _storageService.OpenReadStream(file.StoragePath);
    }

    public async Task SoftDeleteFileAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (file == null)
        {
            throw new FileNotFoundException($"File with id {id} not found");
        }

        await _fileRepository.SoftDeleteAsync(id, cancellationToken);
        _logger.LogInformation("File soft deleted: {FileId}", id);
    }

    public async Task HardDeleteFileAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(id, includeDeleted: true, cancellationToken: cancellationToken);
        if (file == null)
        {
            throw new FileNotFoundException($"File with id {id} not found");
        }

        if (_storageService.FileExists(file.StoragePath))
        {
            _storageService.DeleteFile(file.StoragePath);
        }

        await _fileRepository.HardDeleteAsync(id, cancellationToken);
        _logger.LogInformation("File hard deleted: {FileId}", id);
    }
}

