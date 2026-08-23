namespace BalanceForge.Desktop.ViewModels;

/// <summary>
/// Main window view model.
/// Orchestrates the roster editor UI and application state.
/// </summary>
public class MainWindowViewModel
{
    public string Title { get; } = "BalanceForge - Unit Balance Editor";

    public string StatusMessage { get; } = "Ready";

    public MainWindowViewModel()
    {
    }
}
