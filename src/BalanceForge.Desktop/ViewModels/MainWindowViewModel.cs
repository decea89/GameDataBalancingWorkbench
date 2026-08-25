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
    private RosterUnitViewModel? _comparisonUnitForCtrlClick;
    private int _historyPosition;
    private int _savedHistoryPosition;

    [ObservableProperty]
    private string title = "BalanceForge - Unit Balance Editor";

    [ObservableProperty]
    private string selectedFilePath = string.Empty;

    partial void OnSelectedFilePathChanged(string value)
    {
        SaveCommand.NotifyCanExecuteChanged();
    }

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
    private int displayedUnitCount;

    [ObservableProperty]
    private HashSet<UnitRole> selectedRoles = new(Enum.GetValues<UnitRole>());

    [ObservableProperty]
    private HashSet<int> selectedTiers = new();

    [ObservableProperty]
    private ObservableCollection<FilterOptionViewModel<UnitRole>> roleFilters = new();

    [ObservableProperty]
    private ObservableCollection<FilterOptionViewModel<int>> tierFilters = new();

    [ObservableProperty]
    private RosterUnitViewModel? selectedUnit;

    partial void OnSelectedUnitChanged(RosterUnitViewModel? value)
    {
        if (value != null)
        {
            Inspector.LoadFromUnit(value.UnitDefinition, this);
        }

        if (_comparisonUnitForCtrlClick != null)
        {
            Comparison?.SetComparison(value, _comparisonUnitForCtrlClick);
        }
    }

    [ObservableProperty]
    private UnitInspectorViewModel inspector = new();

    [ObservableProperty]
    private bool isDirty;

    partial void OnIsDirtyChanged(bool value)
    {
        SaveCommand.NotifyCanExecuteChanged();
    }

    partial void OnDisplayedUnitsChanged(ObservableCollection<RosterUnitViewModel> value)
    {
        // Update chart when displayed units change (due to filters or load)
        BalanceChart?.UpdateChartData(value);
    }

    [ObservableProperty]
    private IssuesPanelViewModel issuesPanel = new();

    [ObservableProperty]
    private BalanceChartViewModel? balanceChart;

    [ObservableProperty]
    private ComparisonViewModel? comparison;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool canUndo;

    [ObservableProperty]
    private bool canRedo;

    public IReadOnlyList<UnitRole> AvailableRoles => Enum.GetValues<UnitRole>().ToList();

    public IReadOnlyList<int> AvailableTiers => Enumerable.Range(1, 10).ToList();

    public IReadOnlyList<string> AvailableRoleNames => Enum.GetNames<UnitRole>();

    public MainWindowViewModel()
    {
        // For XAML designer support
        _fileDialogService = null!;
        _loadRosterUseCase = null!;
        _saveRosterUseCase = null!;
        _metricsCalculator = null!;
        _validationService = null!;
        BalanceChart = null!;
        Comparison = null!;
        InitializeDefaultFilters();
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
        BalanceChart = new BalanceChartViewModel(_metricsCalculator);
        Comparison = new ComparisonViewModel(_metricsCalculator, _validationService);
        InitializeDefaultFilters();
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

            InitializeFiltersFromLoadedUnits();

            // Apply filters and populate DisplayedUnits
            ApplyFilters();

            // Reset dirty state and undo/redo after successful load
            IsDirty = false;
            _undoRedoStack.Clear();
            CanUndo = false;
            CanRedo = false;
            _historyPosition = 0;
            _savedHistoryPosition = 0;
            Inspector.ClearUnsavedChanges();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load roster: {ex.Message}";
            Units.Clear();
            DisplayedUnits.Clear();
            DisplayedUnitCount = 0;
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
        DisplayedUnitCount = filtered.Count;
    }

    [RelayCommand]
    public void Undo()
    {
        var command = _undoRedoStack.Undo();
        if (command != null)
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

                    _historyPosition--;
                    IsDirty = _historyPosition != _savedHistoryPosition;

                    if (SelectedUnit?.Id == command.UnitId)
                    {
                        Inspector.LoadFromUnit(unitToUndo, this);
                        if (IsDirty)
                        {
                            Inspector.MarkAsChanged();
                        }
                    }

                    RefreshUnitPresentation(command.UnitId);
                }
            }
        }
    }

    [RelayCommand]
    public void Redo()
    {
        var command = _undoRedoStack.Redo();
        if (command != null)
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

                    _historyPosition++;
                    IsDirty = _historyPosition != _savedHistoryPosition;

                    if (SelectedUnit?.Id == command.UnitId)
                    {
                        Inspector.LoadFromUnit(unitToRedo, this);
                        if (IsDirty)
                        {
                            Inspector.MarkAsChanged();
                        }
                    }

                    RefreshUnitPresentation(command.UnitId);
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
        var unit = Units.FirstOrDefault(candidate => candidate.Id == unitId);
        if (unit == null)
        {
            return;
        }

        var domainPropertyName = propertyName == nameof(UnitInspectorViewModel.UnitRole)
            ? nameof(UnitDefinition.Role)
            : propertyName;
        var property = typeof(UnitDefinition).GetProperty(domainPropertyName);
        if (property?.CanWrite != true)
        {
            return;
        }

        object? convertedValue = newValue;
        if (domainPropertyName == nameof(UnitDefinition.Role))
        {
            if (newValue is not string roleText ||
                !Enum.TryParse<UnitRole>(roleText, ignoreCase: true, out var parsedRole))
            {
                ErrorMessage = $"Invalid role: {newValue}";
                return;
            }

            convertedValue = parsedRole;
        }

        var currentValue = property.GetValue(unit);
        if (Equals(currentValue, convertedValue))
        {
            return;
        }

        if (_undoRedoStack.CanRedo && _savedHistoryPosition > _historyPosition)
        {
            _savedHistoryPosition = int.MinValue;
        }

        property.SetValue(unit, convertedValue);
        var command = new UnitEditCommand(unitId, domainPropertyName, currentValue, convertedValue);
        _undoRedoStack.Push(command);
        _historyPosition++;
        IsDirty = _historyPosition != _savedHistoryPosition;
        StatusMessage = string.Empty;
        ErrorMessage = string.Empty;
        RefreshUnitPresentation(unitId);
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
            Comparison?.SetComparison(null, _comparisonUnitForCtrlClick);
        }
    }

    [RelayCommand]
    public void SelectUnitForComparison(RosterUnitViewModel? unit)
    {
        if (unit == null)
        {
            // Clear comparison
            Comparison?.SetComparison(null, null);
            return;
        }

        // If this is the same as the last comparison unit, toggle it off
        if (_comparisonUnitForCtrlClick?.Id == unit.Id)
        {
            _comparisonUnitForCtrlClick = null;
            Comparison?.SetComparison(null, null);
            return;
        }

        // Otherwise, set or replace the comparison unit
        var previousUnit = _comparisonUnitForCtrlClick;
        _comparisonUnitForCtrlClick = unit;

        // Comparison pairs: selected unit vs stored unit
        Comparison?.SetComparison(SelectedUnit, unit);
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

        RefreshUnitPresentation(unit.Id);
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

        // Refresh comparison if active
        Comparison?.RefreshComparison();
    }

    private void RefreshUnitPresentation(string unitId)
    {
        DisplayedUnits.FirstOrDefault(unit => unit.Id == unitId)?.Refresh();
        BalanceChart?.UpdateChartData(DisplayedUnits);
        RefreshValidationAndMetrics();
    }

    private void InitializeDefaultFilters()
    {
        SetRoleFilters(Enum.GetValues<UnitRole>());
        SetTierFilters(Enumerable.Range(1, 4));
    }

    private void InitializeFiltersFromLoadedUnits()
    {
        var roles = Units
            .Select(unit => unit.Role)
            .Distinct()
            .OrderBy(role => role)
            .ToList();
        var highestTier = Math.Max(4, Units.Select(unit => unit.Tier).DefaultIfEmpty(1).Max());

        SetRoleFilters(roles);
        SetTierFilters(Enumerable.Range(1, highestTier));
    }

    private void SetRoleFilters(IEnumerable<UnitRole> roles)
    {
        var options = roles
            .Select(role => new FilterOptionViewModel<UnitRole>(role))
            .ToList();

        foreach (var option in options)
        {
            option.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName != nameof(FilterOptionViewModel<UnitRole>.IsSelected))
                {
                    return;
                }

                if (option.IsSelected)
                {
                    SelectedRoles.Add(option.Value);
                }
                else
                {
                    SelectedRoles.Remove(option.Value);
                }
            };
        }

        RoleFilters = new ObservableCollection<FilterOptionViewModel<UnitRole>>(options);
        SelectedRoles = options.Select(option => option.Value).ToHashSet();
    }

    private void SetTierFilters(IEnumerable<int> tiers)
    {
        var options = tiers
            .Distinct()
            .OrderBy(tier => tier)
            .Select(tier => new FilterOptionViewModel<int>(tier))
            .ToList();

        foreach (var option in options)
        {
            option.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName != nameof(FilterOptionViewModel<int>.IsSelected))
                {
                    return;
                }

                if (option.IsSelected)
                {
                    SelectedTiers.Add(option.Value);
                }
                else
                {
                    SelectedTiers.Remove(option.Value);
                }
            };
        }

        TierFilters = new ObservableCollection<FilterOptionViewModel<int>>(options);
        SelectedTiers = options.Select(option => option.Value).ToHashSet();
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
            Inspector.LoadFromUnit(affectedUnit.UnitDefinition, this);
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteSave))]
    public async Task Save()
    {
        if (string.IsNullOrEmpty(SelectedFilePath) || !System.IO.File.Exists(SelectedFilePath))
        {
            ErrorMessage = "No file loaded. Please load or save to a file first.";
            return;
        }

        await SaveToFile(SelectedFilePath);
    }

    private bool CanExecuteSave() => IsDirty && !string.IsNullOrEmpty(SelectedFilePath);

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
            _savedHistoryPosition = _historyPosition;
            Inspector.ClearUnsavedChanges();
            StatusMessage = $"✓ Saved to {System.IO.Path.GetFileName(filePath)}";
            ErrorMessage = string.Empty;
        }
        catch (InvalidOperationException ex)
        {
            // Validation errors; units remain in-memory
            ErrorMessage = $"Cannot save due to validation errors: {ex.Message}";
            StatusMessage = string.Empty;
        }
        catch (Exception)
        {
            // Other errors; units remain in-memory
            ErrorMessage = $"Save failed: Unable to write file. Please check the file path and permissions.";
            StatusMessage = string.Empty;
        }
    }
}
