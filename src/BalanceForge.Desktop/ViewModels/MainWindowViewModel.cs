namespace BalanceForge.Desktop.ViewModels;

using System.Collections.ObjectModel;
using BalanceForge.Application;
using BalanceForge.Application.Services;
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
    private readonly BalanceMetricsCalculator _metricsCalculator;
    private readonly UnitValidationService _validationService;

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

    [ObservableProperty]
    private ObservableCollection<RosterUnitViewModel> displayedUnits = new();

    [ObservableProperty]
    private HashSet<UnitRole> selectedRoles = new(Enum.GetValues<UnitRole>());

    [ObservableProperty]
    private HashSet<int> selectedTiers = new();

    [ObservableProperty]
    private RosterUnitViewModel? selectedUnit;

    [ObservableProperty]
    private UnitInspectorViewModel inspector = new();

    [ObservableProperty]
    private bool isDirty;

    public IReadOnlyList<UnitRole> AvailableRoles => Enum.GetValues<UnitRole>().ToList();

    public IReadOnlyList<int> AvailableTiers => Enumerable.Range(1, 10).ToList();

    public MainWindowViewModel()
    {
        // For XAML designer support
        _fileDialogService = null!;
        _loadRosterUseCase = null!;
        _metricsCalculator = null!;
        _validationService = null!;
    }

    public MainWindowViewModel(
        IFileDialogService fileDialogService,
        ILoadRosterUseCase loadRosterUseCase,
        BalanceMetricsCalculator metricsCalculator,
        UnitValidationService validationService)
    {
        _fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
        _loadRosterUseCase = loadRosterUseCase ?? throw new ArgumentNullException(nameof(loadRosterUseCase));
        _metricsCalculator = metricsCalculator ?? throw new ArgumentNullException(nameof(metricsCalculator));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        Inspector.PropertyChanged += (s, e) => IsDirty = Inspector.HasUnsavedChanges;
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

            // Initialize tiers from loaded units
            var loadedTiers = Units.Select(u => u.Tier).Distinct().ToHashSet();
            SelectedTiers = new HashSet<int>(loadedTiers);

            // Apply filters and populate DisplayedUnits
            ApplyFilters();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load roster: {ex.Message}";
            Units.Clear();
            DisplayedUnits.Clear();
            LoadedUnitCount = 0;
            ValidationIssueCount = 0;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void ApplyFilters()
    {
        var filtered = Units
            .Where(u => SelectedRoles.Contains(u.Role) && SelectedTiers.Contains(u.Tier))
            .Select(u => new RosterUnitViewModel(u, _metricsCalculator))
            .ToList();

        DisplayedUnits = new ObservableCollection<RosterUnitViewModel>(filtered);
    }

    [RelayCommand]
    public void SelectUnit(RosterUnitViewModel? unit)
    {
        SelectedUnit = unit;
        if (unit != null)
        {
            Inspector.LoadFromUnit(unit.UnitDefinition);
        }
        else
        {
            // Clear inspector
            Inspector = new UnitInspectorViewModel();
        }
    }

    [RelayCommand]
    public void UpdateUnit()
    {
        if (SelectedUnit?.UnitDefinition == null)
            return;

        var unit = SelectedUnit.UnitDefinition;

        // Try parsing role
        if (!Enum.TryParse<UnitRole>(Inspector.UnitRole, ignoreCase: true, out var newRole))
        {
            ErrorMessage = $"Invalid role: {Inspector.UnitRole}";
            return;
        }

        // Update unit properties
        unit.DisplayName = Inspector.DisplayName;
        unit.Role = newRole;
        unit.Tier = Inspector.Tier;
        unit.Health = Inspector.Health;
        unit.Damage = Inspector.Damage;
        unit.AttacksPerSecond = Inspector.AttacksPerSecond;
        unit.Armor = Inspector.Armor;
        unit.Range = Inspector.Range;
        unit.WoodCost = Inspector.WoodCost;
        unit.GoldCost = Inspector.GoldCost;
        unit.PopulationCost = Inspector.PopulationCost;
        unit.ProductionTimeSeconds = Inspector.ProductionTimeSeconds;

        // Revalidate all units
        var allIssues = Units
            .SelectMany(u => _validationService.Validate(u))
            .Concat(_validationService.ValidateRoster(Units.ToList()))
            .ToList();

        ValidationIssueCount = allIssues.Count;
        ErrorMessage = string.Empty;

        // Update roster with the edited unit
        ApplyFilters();
        Inspector.ClearUnsavedChanges();
        IsDirty = false;
    }
}
