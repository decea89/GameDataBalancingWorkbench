namespace BalanceForge.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>
/// Inspector view model for viewing and editing a single unit.
/// Tracks edit state and validation results in-memory.
/// Calls back to MainWindowViewModel to record edits in the undo/redo stack.
/// </summary>
public partial class UnitInspectorViewModel : ObservableObject
{
    private string? _currentUnitId;
    private MainWindowViewModel? _mainViewModel;
    private bool _isLoadingFromUnit;
    [ObservableProperty]
    private string displayName = string.Empty;

    private string _previousDisplayName = string.Empty;

    partial void OnDisplayNameChanged(string value)
    {
        if (!_isLoadingFromUnit && _currentUnitId != null && _mainViewModel != null)
        {
            _mainViewModel.RecordUnitEdit(_currentUnitId, nameof(DisplayName), _previousDisplayName, value);
            _previousDisplayName = value;
            HasUnsavedChanges = true;
        }
    }

    [ObservableProperty]
    private string unitRole = string.Empty;

    private string _previousUnitRole = string.Empty;

    partial void OnUnitRoleChanged(string value)
    {
        if (!_isLoadingFromUnit && _currentUnitId != null && _mainViewModel != null)
        {
            _mainViewModel.RecordUnitEdit(_currentUnitId, nameof(UnitRole), _previousUnitRole, value);
            _previousUnitRole = value;
            HasUnsavedChanges = true;
        }
    }

    [ObservableProperty]
    private int tier;

    private int _previousTier;

    partial void OnTierChanged(int value)
    {
        if (!_isLoadingFromUnit && _currentUnitId != null && _mainViewModel != null)
        {
            _mainViewModel.RecordUnitEdit(_currentUnitId, nameof(Tier), _previousTier, value);
            _previousTier = value;
            HasUnsavedChanges = true;
        }
    }

    [ObservableProperty]
    private double health;

    private double _previousHealth;

    partial void OnHealthChanged(double value)
    {
        if (!_isLoadingFromUnit && _currentUnitId != null && _mainViewModel != null)
        {
            _mainViewModel.RecordUnitEdit(_currentUnitId, nameof(Health), _previousHealth, value);
            _previousHealth = value;
            HasUnsavedChanges = true;
        }
    }

    [ObservableProperty]
    private double damage;

    private double _previousDamage;

    partial void OnDamageChanged(double value)
    {
        if (!_isLoadingFromUnit && _currentUnitId != null && _mainViewModel != null)
        {
            _mainViewModel.RecordUnitEdit(_currentUnitId, nameof(Damage), _previousDamage, value);
            _previousDamage = value;
            HasUnsavedChanges = true;
        }
    }

    [ObservableProperty]
    private double attacksPerSecond;

    private double _previousAttacksPerSecond;

    partial void OnAttacksPerSecondChanged(double value)
    {
        if (!_isLoadingFromUnit && _currentUnitId != null && _mainViewModel != null)
        {
            _mainViewModel.RecordUnitEdit(_currentUnitId, nameof(AttacksPerSecond), _previousAttacksPerSecond, value);
            _previousAttacksPerSecond = value;
            HasUnsavedChanges = true;
        }
    }

    [ObservableProperty]
    private double armor;

    private double _previousArmor;

    partial void OnArmorChanged(double value)
    {
        if (!_isLoadingFromUnit && _currentUnitId != null && _mainViewModel != null)
        {
            _mainViewModel.RecordUnitEdit(_currentUnitId, nameof(Armor), _previousArmor, value);
            _previousArmor = value;
            HasUnsavedChanges = true;
        }
    }

    [ObservableProperty]
    private double range;

    private double _previousRange;

    partial void OnRangeChanged(double value)
    {
        if (!_isLoadingFromUnit && _currentUnitId != null && _mainViewModel != null)
        {
            _mainViewModel.RecordUnitEdit(_currentUnitId, nameof(Range), _previousRange, value);
            _previousRange = value;
            HasUnsavedChanges = true;
        }
    }

    [ObservableProperty]
    private int woodCost;

    private int _previousWoodCost;

    partial void OnWoodCostChanged(int value)
    {
        if (!_isLoadingFromUnit && _currentUnitId != null && _mainViewModel != null)
        {
            _mainViewModel.RecordUnitEdit(_currentUnitId, nameof(WoodCost), _previousWoodCost, value);
            _previousWoodCost = value;
            HasUnsavedChanges = true;
        }
    }

    [ObservableProperty]
    private int goldCost;

    private int _previousGoldCost;

    partial void OnGoldCostChanged(int value)
    {
        if (!_isLoadingFromUnit && _currentUnitId != null && _mainViewModel != null)
        {
            _mainViewModel.RecordUnitEdit(_currentUnitId, nameof(GoldCost), _previousGoldCost, value);
            _previousGoldCost = value;
            HasUnsavedChanges = true;
        }
    }

    [ObservableProperty]
    private int populationCost;

    private int _previousPopulationCost;

    partial void OnPopulationCostChanged(int value)
    {
        if (!_isLoadingFromUnit && _currentUnitId != null && _mainViewModel != null)
        {
            _mainViewModel.RecordUnitEdit(_currentUnitId, nameof(PopulationCost), _previousPopulationCost, value);
            _previousPopulationCost = value;
            HasUnsavedChanges = true;
        }
    }

    [ObservableProperty]
    private double productionTimeSeconds;

    private double _previousProductionTimeSeconds;

    partial void OnProductionTimeSecondsChanged(double value)
    {
        if (!_isLoadingFromUnit && _currentUnitId != null && _mainViewModel != null)
        {
            _mainViewModel.RecordUnitEdit(_currentUnitId, nameof(ProductionTimeSeconds), _previousProductionTimeSeconds, value);
            _previousProductionTimeSeconds = value;
            HasUnsavedChanges = true;
        }
    }

    [ObservableProperty]
    private bool hasUnsavedChanges;

    public void LoadFromUnit(BalanceForge.Domain.UnitDefinition unit, MainWindowViewModel? mainViewModel = null)
    {
        _isLoadingFromUnit = true;
        _currentUnitId = unit.Id;
        _mainViewModel = mainViewModel;

        DisplayName = unit.DisplayName;
        _previousDisplayName = unit.DisplayName;

        UnitRole = unit.Role.ToString();
        _previousUnitRole = unit.Role.ToString();

        Tier = unit.Tier;
        _previousTier = unit.Tier;

        Health = unit.Health;
        _previousHealth = unit.Health;

        Damage = unit.Damage;
        _previousDamage = unit.Damage;

        AttacksPerSecond = unit.AttacksPerSecond;
        _previousAttacksPerSecond = unit.AttacksPerSecond;

        Armor = unit.Armor;
        _previousArmor = unit.Armor;

        Range = unit.Range;
        _previousRange = unit.Range;

        WoodCost = unit.WoodCost;
        _previousWoodCost = unit.WoodCost;

        GoldCost = unit.GoldCost;
        _previousGoldCost = unit.GoldCost;

        PopulationCost = unit.PopulationCost;
        _previousPopulationCost = unit.PopulationCost;

        ProductionTimeSeconds = unit.ProductionTimeSeconds;
        _previousProductionTimeSeconds = unit.ProductionTimeSeconds;

        HasUnsavedChanges = false;
        _isLoadingFromUnit = false;
    }

    public void ClearUnsavedChanges()
    {
        HasUnsavedChanges = false;
    }

    public void MarkAsChanged()
    {
        HasUnsavedChanges = true;
    }
}
