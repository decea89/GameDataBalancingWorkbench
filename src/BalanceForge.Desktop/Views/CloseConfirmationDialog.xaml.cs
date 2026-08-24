namespace BalanceForge.Desktop.Views;

using BalanceForge.Desktop.ViewModels;
using System.Windows;

/// <summary>
/// Interaction logic for CloseConfirmationDialog.xaml
/// </summary>
public partial class CloseConfirmationDialog : Window
{
    public CloseConfirmationDialog()
    {
        InitializeComponent();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        // Set DialogResult based on ViewModel state
        if (DataContext is CloseConfirmationDialogViewModel viewModel && viewModel.DialogResult.HasValue)
        {
            DialogResult = viewModel.DialogResult.Value;
        }
    }
}
