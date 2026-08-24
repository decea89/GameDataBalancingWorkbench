namespace BalanceForge.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

/// <summary>
/// View model for the close confirmation dialog.
/// Allows user to save, discard, or cancel when closing with unsaved changes.
/// </summary>
public partial class CloseConfirmationDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private bool? dialogResult;

    /// <summary>
    /// User chose to save before closing.
    /// </summary>
    [RelayCommand]
    public void Save()
    {
        DialogResult = true;
    }

    /// <summary>
    /// User chose to discard changes and close.
    /// </summary>
    [RelayCommand]
    public void Discard()
    {
        DialogResult = false;
    }

    /// <summary>
    /// User chose to cancel closing.
    /// </summary>
    [RelayCommand]
    public void Cancel()
    {
        DialogResult = null;
    }
}
