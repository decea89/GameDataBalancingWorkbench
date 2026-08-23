namespace BalanceForge.Desktop.Services;

/// <summary>
/// Abstraction for file dialog interactions, enabling testability and platform flexibility.
/// </summary>
public interface IFileDialogService
{
    /// <summary>
    /// Opens a file selection dialog for JSON files.
    /// Returns the selected file path, or null if the user cancels.
    /// </summary>
    Task<string?> OpenFileAsync(string? initialDirectory = null);
}
