namespace BalanceForge.Infrastructure.Services;

/// <summary>
/// Real implementation of IFileAccessor wrapping System.IO operations.
/// </summary>
public class FileAccessor : IFileAccessor
{
    /// <inheritdoc />
    public async Task<string> ReadAllTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        return await File.ReadAllTextAsync(filePath, cancellationToken);
    }

    /// <inheritdoc />
    public async Task WriteAllTextAsync(string filePath, string content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        ArgumentNullException.ThrowIfNull(content);

        // Ensure the directory exists
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(filePath, content, cancellationToken);
    }

    /// <inheritdoc />
    public bool FileExists(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        return File.Exists(filePath);
    }
}
