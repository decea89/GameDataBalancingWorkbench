namespace BalanceForge.Desktop;

using BalanceForge.Application;
using BalanceForge.Application.Services;
using BalanceForge.Application.UseCases;
using BalanceForge.Desktop.Services;
using BalanceForge.Desktop.ViewModels;
using BalanceForge.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Composition root for dependency injection setup.
/// Wires Domain → Application → Infrastructure layers with the Desktop UI.
/// </summary>
public static class CompositionRoot
{
    /// <summary>
    /// Builds the IServiceProvider with all registered services.
    /// </summary>
    public static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        // Infrastructure services
        services.AddSingleton<IFileAccessor, FileAccessor>();
        services.AddSingleton<IUnitRosterService, FileBasedUnitRosterService>();

        // Application services
        services.AddSingleton<UnitValidationService>();
        services.AddSingleton<BalanceMetricsCalculator>();

        // Desktop services
        services.AddSingleton<IFileDialogService, FileDialogService>();

        // Use cases
        services.AddSingleton<IValidateRosterUseCase, ValidateRosterUseCase>();
        services.AddSingleton<ILoadRosterUseCase, LoadRosterUseCase>();
        services.AddSingleton<ISaveRosterUseCase, SaveRosterUseCase>();

        // ViewModels
        services.AddSingleton<MainWindowViewModel>();

        return services.BuildServiceProvider();
    }
}
