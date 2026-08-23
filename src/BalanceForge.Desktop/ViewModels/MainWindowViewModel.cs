namespace BalanceForge.Desktop.ViewModels;

using System.Collections.ObjectModel;
using BalanceForge.Application.UseCases;
using BalanceForge.Desktop.Services;
using BalanceForge.Domain;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

/// <summary>
/// Main window view model.
/// Orchestrates the roster editor UI and application state.
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IFileDialogService _fileDialogService;
    private readonly ILoadRosterUseCase _loadRosterUseCase;

    [ObservableProperty]
    private string title = "BalanceForge - Unit Balance Editor";

    [ObservableProperty]
    private string selectedFilePath = string.Empty;

    [ObservableProperty]
    private int loadedUnitCount = 0;

    [ObservableProperty]
    private int validationIssueCount = 0;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<UnitDefinition> units = new();

    public MainWindowViewModel()
    {
        // For XAML designer support
        _fileDialogService = null!;
        _loadRosterUseCase = null!;
    }

    public MainWindowViewModel(
        IFileDialogService fileDialogService,
        ILoadRosterUseCase loadRosterUseCase)
    {
        _fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
        _loadRosterUseCase = loadRosterUseCase ?? throw new ArgumentNullException(nameof(loadRosterUseCase));
    }

    [RelayCommand]
    public async Task SelectFile()
    {
        try
        {
            var filePath = await _fileDialogService.OpenFileAsync();
            if (!string.IsNullOrEmpty(filePath))
            {
                SelectedFilePath = filePath;
                ErrorMessage = string.Empty;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to open file dialog: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task Load()
    {
        if (string.IsNullOrEmpty(SelectedFilePath))
        {
            ErrorMessage = "Please select a file first.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _loadRosterUseCase.ExecuteAsync(SelectedFilePath);

            if (result.HasErrors)
            {
                ErrorMessage = $"Loaded with errors: {result.ValidationIssues.Count(v => v.Severity == BalanceForge.Domain.ValidationSeverity.Error)} errors";
            }

            Units = new ObservableCollection<UnitDefinition>(result.Units);
            LoadedUnitCount = result.Units.Count;
            ValidationIssueCount = result.ValidationIssues.Count;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load roster: {ex.Message}";
            Units.Clear();
            LoadedUnitCount = 0;
            ValidationIssueCount = 0;
        }
        finally
        {
            IsLoading = false;
        }
    }
}

