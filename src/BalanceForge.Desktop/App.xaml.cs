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
        MainWindow.Show();
    }
}

