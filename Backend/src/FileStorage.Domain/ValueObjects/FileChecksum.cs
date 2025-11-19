namespace FileStorage.Domain.ValueObjects;
public record FileChecksum
{
    public string Value { get; init; }

    private FileChecksum(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Checksum cannot be empty", nameof(value));
        
        if (value.Length != 64)
            throw new ArgumentException("Invalid SHA-256 checksum format", nameof(value));

        Value = value.ToLowerInvariant();
    }

    public static FileChecksum Create(string value) => new(value);

    public static async Task<FileChecksum> ComputeAsync(Stream stream)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(stream);
        var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        return new FileChecksum(hashString);
    }

    public override string ToString() => Value;
}

