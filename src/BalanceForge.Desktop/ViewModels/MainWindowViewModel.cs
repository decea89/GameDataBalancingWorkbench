namespace BalanceForge.Desktop.ViewModels;

using System.Collections.ObjectModel;
using BalanceForge.Application;
using BalanceForge.Application.Services;
using BalanceForge.Application.UndoRedo;
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
    private readonly ISaveRosterUseCase _saveRosterUseCase;
    private readonly BalanceMetricsCalculator _metricsCalculator;
    private readonly UnitValidationService _validationService;
    private readonly UndoRedoStack _undoRedoStack = new();

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

    [ObservableProperty]
    private IssuesPanelViewModel issuesPanel = new();

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool canUndo;

    [ObservableProperty]
    private bool canRedo;

    public IReadOnlyList<UnitRole> AvailableRoles => Enum.GetValues<UnitRole>().ToList();

    public IReadOnlyList<int> AvailableTiers => Enumerable.Range(1, 10).ToList();

    public MainWindowViewModel()
    {
        // For XAML designer support
        _fileDialogService = null!;
        _loadRosterUseCase = null!;
        _saveRosterUseCase = null!;
        _metricsCalculator = null!;
        _validationService = null!;
    }

    public MainWindowViewModel(
        IFileDialogService fileDialogService,
        ILoadRosterUseCase loadRosterUseCase,
        ISaveRosterUseCase saveRosterUseCase,
        BalanceMetricsCalculator metricsCalculator,
        UnitValidationService validationService)
    {
        _fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
        _loadRosterUseCase = loadRosterUseCase ?? throw new ArgumentNullException(nameof(loadRosterUseCase));
        _saveRosterUseCase = saveRosterUseCase ?? throw new ArgumentNullException(nameof(saveRosterUseCase));
        _metricsCalculator = metricsCalculator ?? throw new ArgumentNullException(nameof(metricsCalculator));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        Inspector.PropertyChanged += (s, e) => IsDirty = Inspector.HasUnsavedChanges;
        _undoRedoStack.StackChanged += (s, e) =>
        {
            CanUndo = _undoRedoStack.CanUndo;
            CanRedo = _undoRedoStack.CanRedo;
        };
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
            IssuesPanel.UpdateIssues(result.ValidationIssues);

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
    public void Undo()
    {
        var command = _undoRedoStack.Undo();
        if (command != null && SelectedUnit != null)
        {
            // Find the unit in our collection and restore its old value
            var unitToUndo = Units.FirstOrDefault(u => u.Id == command.UnitId);
            if (unitToUndo != null)
            {
                // Use reflection to set the property to its old value
                var property = typeof(UnitDefinition).GetProperty(command.PropertyName);
                if (property?.CanWrite == true)
                {
                    property.SetValue(unitToUndo, command.OldValue);

                    // Reload inspector with the reverted unit
                    var displayUnit = DisplayedUnits.FirstOrDefault(u => u.Id == command.UnitId);
                    if (displayUnit != null)
                    {
                        Inspector.LoadFromUnit(unitToUndo, this);
                        // Revalidate and update metrics
                        RefreshValidationAndMetrics();
                    }
                }
            }
        }
    }

    [RelayCommand]
    public void Redo()
    {
        var command = _undoRedoStack.Redo();
        if (command != null && SelectedUnit != null)
        {
            // Find the unit in our collection and restore its new value
            var unitToRedo = Units.FirstOrDefault(u => u.Id == command.UnitId);
            if (unitToRedo != null)
            {
                // Use reflection to set the property to its new value
                var property = typeof(UnitDefinition).GetProperty(command.PropertyName);
                if (property?.CanWrite == true)
                {
                    property.SetValue(unitToRedo, command.NewValue);

                    // Reload inspector with the redone unit
                    var displayUnit = DisplayedUnits.FirstOrDefault(u => u.Id == command.UnitId);
                    if (displayUnit != null)
                    {
                        Inspector.LoadFromUnit(unitToRedo, this);
                        // Revalidate and update metrics
                        RefreshValidationAndMetrics();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Records a field edit in the undo/redo stack.
    /// Called by UnitInspectorViewModel when a field changes.
    /// </summary>
    public void RecordUnitEdit(string unitId, string propertyName, object? oldValue, object? newValue)
    {
        var command = new UnitEditCommand(unitId, propertyName, oldValue, newValue);
        _undoRedoStack.Push(command);
    }

    [RelayCommand]
    public void SelectUnit(RosterUnitViewModel? unit)
    {
        SelectedUnit = unit;
        if (unit != null)
        {
            Inspector.LoadFromUnit(unit.UnitDefinition, this);
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
        RefreshValidationAndMetrics();

        // Update roster with the edited unit
        ApplyFilters();
        Inspector.ClearUnsavedChanges();
        IsDirty = false;
    }

    private void RefreshValidationAndMetrics()
    {
        var allIssues = Units
            .SelectMany(u => _validationService.Validate(u))
            .Concat(_validationService.ValidateRoster(Units.ToList()))
            .ToList();

        ValidationIssueCount = allIssues.Count;
        ErrorMessage = string.Empty;
        IssuesPanel.UpdateIssues(allIssues);
    }

    [RelayCommand]
    public void SelectIssue(ValidationIssue? issue)
    {
        if (issue == null)
            return;

        // Find and select the unit affected by this issue
        var affectedUnit = DisplayedUnits.FirstOrDefault(u => u.Id == issue.UnitId);
        if (affectedUnit != null)
        {
            SelectedUnit = affectedUnit;
            Inspector.LoadFromUnit(affectedUnit.UnitDefinition);
        }
    }

    [RelayCommand]
    public async Task Save()
    {
        if (string.IsNullOrEmpty(SelectedFilePath) || !System.IO.File.Exists(SelectedFilePath))
        {
            ErrorMessage = "No file loaded. Please load or save to a file first.";
            return;
        }

        await SaveToFile(SelectedFilePath);
    }

    [RelayCommand]
    public async Task SaveAs()
    {
        var filePath = await _fileDialogService.OpenFileAsync();
        if (!string.IsNullOrEmpty(filePath))
        {
            SelectedFilePath = filePath;
            await SaveToFile(filePath);
        }
    }

    private async Task SaveToFile(string filePath)
    {
        try
        {
            StatusMessage = "Saving...";
            ErrorMessage = string.Empty;

            await _saveRosterUseCase.ExecuteAsync(filePath, Units.ToList());

            // Success
            IsDirty = false;
            Inspector.ClearUnsavedChanges();
            StatusMessage = $"Saved successfully to {System.IO.Path.GetFileName(filePath)}";
            ErrorMessage = string.Empty;
        }
        catch (InvalidOperationException ex)
        {
            // Validation errors; units remain in-memory
            ErrorMessage = $"Cannot save: {ex.Message}";
            StatusMessage = string.Empty;
        }
        catch (Exception ex)
        {
            // Other errors; units remain in-memory
            ErrorMessage = $"Save failed: {ex.Message}";
            StatusMessage = string.Empty;
        }
    }
}
