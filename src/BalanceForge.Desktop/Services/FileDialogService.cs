namespace BalanceForge.Desktop.Services;

using Microsoft.Win32;

/// <summary>
/// WPF-based file dialog service implementation.
/// </summary>
public class FileDialogService : IFileDialogService
{
    public Task<string?> OpenFileAsync(string? initialDirectory = null)
    {
        return Task.Run(() =>
        {
            var dialog = new OpenFileDialog
            {
                Title = "Open units.json",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                DefaultExt = ".json",
                InitialDirectory = initialDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Multiselect = false
            };

            var result = dialog.ShowDialog();
            return result == true ? dialog.FileName : null;
        });
    }
}
