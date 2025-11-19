using FileStorage.Domain.Entities;

namespace FileStorage.Domain.Repositories;

public interface IFileMetadataRepository
{
    Task<FileMetadata?> GetByIdAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<FileMetadata>> GetAllAsync(int skip, int take, bool includeDeleted = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<FileMetadata>> SearchAsync(string searchTerm, int skip, int take, bool includeDeleted = false, CancellationToken cancellationToken = default);
    Task<FileMetadata> AddAsync(FileMetadata fileMetadata, CancellationToken cancellationToken = default);
    Task UpdateAsync(FileMetadata fileMetadata, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task HardDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(bool includeDeleted = false, CancellationToken cancellationToken = default);
}

