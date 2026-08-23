namespace BalanceForge.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>
/// Inspector view model for viewing and editing a single unit.
/// Tracks edit state and validation results in-memory.
/// </summary>
public partial class UnitInspectorViewModel : ObservableObject
{
    [ObservableProperty]
    private string displayName = string.Empty;

    [ObservableProperty]
    private string unitRole = string.Empty;

    [ObservableProperty]
    private int tier;

    [ObservableProperty]
    private double health;

    [ObservableProperty]
    private double damage;

    [ObservableProperty]
    private double attacksPerSecond;

    [ObservableProperty]
    private double armor;

    [ObservableProperty]
    private double range;

    [ObservableProperty]
    private int woodCost;

    [ObservableProperty]
    private int goldCost;

    [ObservableProperty]
    private int populationCost;

    [ObservableProperty]
    private double productionTimeSeconds;

    [ObservableProperty]
    private bool hasUnsavedChanges;

    public void LoadFromUnit(BalanceForge.Domain.UnitDefinition unit)
    {
        DisplayName = unit.DisplayName;
        UnitRole = unit.Role.ToString();
        Tier = unit.Tier;
        Health = unit.Health;
        Damage = unit.Damage;
        AttacksPerSecond = unit.AttacksPerSecond;
        Armor = unit.Armor;
        Range = unit.Range;
        WoodCost = unit.WoodCost;
        GoldCost = unit.GoldCost;
        PopulationCost = unit.PopulationCost;
        ProductionTimeSeconds = unit.ProductionTimeSeconds;
        HasUnsavedChanges = false;
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
