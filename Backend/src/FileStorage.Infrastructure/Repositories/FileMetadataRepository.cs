using FileStorage.Domain.Entities;
using FileStorage.Domain.Repositories;
using FileStorage.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FileStorage.Infrastructure.Repositories;


public class FileMetadataRepository : IFileMetadataRepository
{
    private readonly ApplicationDbContext _context;

    public FileMetadataRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FileMetadata?> GetByIdAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Files.AsQueryable();

        if (!includeDeleted)
        {
            query = query.Where(f => f.DeletedAt == null);
        }

        return await query
            .Include(f => f.CreatedByUser)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<FileMetadata>> GetAllAsync(int skip, int take, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Files.AsQueryable();

        if (!includeDeleted)
        {
            query = query.Where(f => f.DeletedAt == null);
        }

        return await query
            .Include(f => f.CreatedByUser)
            .OrderByDescending(f => f.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<FileMetadata>> SearchAsync(string searchTerm, int skip, int take, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Files.AsQueryable();

        if (!includeDeleted)
        {
            query = query.Where(f => f.DeletedAt == null);
        }

        var lowerSearchTerm = searchTerm.ToLowerInvariant();
        query = query.Where(f =>
            f.OriginalFileName.ToLower().Contains(lowerSearchTerm) ||
            (f.Tags != null && f.Tags.ToLower().Contains(lowerSearchTerm)) ||
            f.ContentType.ToLower().Contains(lowerSearchTerm));

        return await query
            .Include(f => f.CreatedByUser)
            .OrderByDescending(f => f.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<FileMetadata> AddAsync(FileMetadata fileMetadata, CancellationToken cancellationToken = default)
    {
        await _context.Files.AddAsync(fileMetadata, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return fileMetadata;
    }

    public async Task UpdateAsync(FileMetadata fileMetadata, CancellationToken cancellationToken = default)
    {
        fileMetadata.UpdatedAt = DateTime.UtcNow;
        _context.Files.Update(fileMetadata);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var file = await _context.Files.FindAsync(new object[] { id }, cancellationToken);
        if (file != null && file.DeletedAt == null)
        {
            file.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task HardDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var file = await _context.Files.FindAsync(new object[] { id }, cancellationToken);
        if (file != null)
        {
            _context.Files.Remove(file);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Files.AnyAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<int> CountAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Files.AsQueryable();
        if (!includeDeleted)
        {
            query = query.Where(f => f.DeletedAt == null);
        }
        return await query.CountAsync(cancellationToken);
    }
}

