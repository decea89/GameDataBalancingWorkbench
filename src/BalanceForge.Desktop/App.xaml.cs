using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace BalanceForge.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public System.IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        // Build the service provider
        ServiceProvider = CompositionRoot.BuildServiceProvider();

        // Set the main window
        MainWindow = new MainWindow();
        MainWindow.Closing += OnMainWindowClosing;
        MainWindow.Show();
    }

    private void OnMainWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (MainWindow.DataContext is ViewModels.MainWindowViewModel viewModel && viewModel.IsDirty)
        {
            // Show confirmation dialog
            var confirmDialog = new Views.CloseConfirmationDialog
            {
                Owner = MainWindow
            };

            bool? result = confirmDialog.ShowDialog();

            if (result == true)
            {
                // User clicked Save
                viewModel.SaveCommand.Execute(null);
                // If save failed, ErrorMessage will be set; we should stay open
                if (viewModel.IsDirty)
                {
                    e.Cancel = true;
                    return;
                }
            }
            else if (result == false)
            {
                // User clicked Discard - allow close
            }
            else
            {
                // User clicked Cancel or closed dialog - prevent close
                e.Cancel = true;
            }
        }
    }
}
