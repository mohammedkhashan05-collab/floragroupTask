namespace FileStorage.Domain.Repositories;

public interface IFileStorageService
{
    string GenerateStoragePath(Guid fileId);
    Task<string> SaveFileAsync(Guid fileId, Stream fileStream, CancellationToken cancellationToken = default);
    Stream OpenReadStream(string storagePath);
    void DeleteFile(string storagePath);
    bool FileExists(string storagePath);
    Task<string> ComputeChecksumAsync(Stream stream);
}

