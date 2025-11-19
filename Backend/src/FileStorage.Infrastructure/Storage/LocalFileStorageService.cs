using System.Security.Cryptography;
using FileStorage.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FileStorage.Infrastructure.Storage;


public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storageRoot;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
    {
        _storageRoot = configuration["Storage:RootPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "_storage");
        _logger = logger;

        if (!Directory.Exists(_storageRoot))
        {
            Directory.CreateDirectory(_storageRoot);
            _logger.LogInformation("Created storage root directory: {StorageRoot}", _storageRoot);
        }
    }

   
    public string GenerateStoragePath(Guid fileId)
    {
        var now = DateTime.UtcNow;
        return Path.Combine(
            now.Year.ToString("D4"),
            now.Month.ToString("D2"),
            now.Day.ToString("D2"),
            fileId.ToString()
        ).Replace('\\', '/');
    }

   
    private string GetFullPath(string storagePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_storageRoot, storagePath));
        var rootPath = Path.GetFullPath(_storageRoot);

        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Invalid storage path: directory traversal detected");
        }

        return fullPath;
    }

    
    public async Task<string> SaveFileAsync(Guid fileId, Stream fileStream, CancellationToken cancellationToken = default)
    {
        var storagePath = GenerateStoragePath(fileId);
        var fullDirectoryPath = GetFullPath(storagePath);
        var contentPath = Path.Combine(fullDirectoryPath, "content.bin");
        var tempPath = Path.Combine(fullDirectoryPath, $"content.tmp.{Guid.NewGuid()}");

        try
        {
            Directory.CreateDirectory(fullDirectoryPath);

            using (var fileStreamOut = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, FileOptions.SequentialScan))
            {
                await fileStream.CopyToAsync(fileStreamOut, cancellationToken);
                await fileStreamOut.FlushAsync(cancellationToken);
            }

            File.Move(tempPath, contentPath, overwrite: true);

            _logger.LogInformation("File saved successfully: {StoragePath}", storagePath);
            return storagePath;
        }
        catch
        {
            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch { }
            }
            throw;
        }
    }


    public Stream OpenReadStream(string storagePath)
    {
        var fullPath = GetFullPath(Path.Combine(storagePath, "content.bin"));
        
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found at storage path: {storagePath}");
        }

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, FileOptions.SequentialScan);
    }

    public void DeleteFile(string storagePath)
    {
        var fullPath = GetFullPath(storagePath);
        var contentPath = Path.Combine(fullPath, "content.bin");
        var metadataPath = Path.Combine(fullPath, "metadata.json");

        try
        {
            if (File.Exists(contentPath))
            {
                File.Delete(contentPath);
            }

            if (File.Exists(metadataPath))
            {
                File.Delete(metadataPath);
            }

            try
            {
                if (Directory.Exists(fullPath) && !Directory.EnumerateFileSystemEntries(fullPath).Any())
                {
                    Directory.Delete(fullPath);
                }
            }
            catch
            {
            }

            _logger.LogInformation("File deleted from storage: {StoragePath}", storagePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from storage: {StoragePath}", storagePath);
            throw;
        }
    }


    public bool FileExists(string storagePath)
    {
        var fullPath = GetFullPath(Path.Combine(storagePath, "content.bin"));
        return File.Exists(fullPath);
    }

    public async Task<string> ComputeChecksumAsync(Stream stream)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}

