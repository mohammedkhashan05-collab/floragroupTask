namespace FileStorage.Domain.Entities;

/// <summary>
/// Represents file metadata stored in the database.
/// Uses server-generated GUID keys instead of filename-based paths for security.
/// </summary>
public class FileMetadata
{
    public Guid Id { get; set; }
    public string? OriginalFileName { get; set; } 
    public string? ContentType { get; set; } 
    public long SizeInBytes { get; set; }
    public string? Checksum { get; set; } 
    public string? StoragePath { get; set; } 
    public string? Tags { get; set; } 
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; } 
    public bool IsDeleted => DeletedAt.HasValue;

    public User? CreatedByUser { get; set; }
}

