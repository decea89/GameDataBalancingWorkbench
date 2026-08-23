namespace BalanceForge.Infrastructure.Services;

/// <summary>
/// Abstraction for file I/O operations. Allows testing and potential future alternative implementations.
/// </summary>
public interface IFileAccessor
{
    /// <summary>
    /// Reads all text from a file asynchronously.
    /// </summary>
    Task<string> ReadAllTextAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes text to a file asynchronously. Creates the file if it doesn't exist; overwrites if it does.
    /// </summary>
    Task WriteAllTextAsync(string filePath, string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    bool FileExists(string filePath);
}
