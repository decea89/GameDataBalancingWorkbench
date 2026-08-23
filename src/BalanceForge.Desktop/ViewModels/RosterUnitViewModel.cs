namespace BalanceForge.Desktop.ViewModels;

using BalanceForge.Application;
using BalanceForge.Domain;

/// <summary>
/// View model wrapper for a unit definition with calculated metrics.
/// Used to expose both domain model and calculated values for UI binding.
/// </summary>
public class RosterUnitViewModel
{
    private readonly UnitDefinition _unit;
    private readonly BalanceMetricsCalculator _calculator;

    public RosterUnitViewModel(UnitDefinition unit, BalanceMetricsCalculator calculator)
    {
        _unit = unit ?? throw new ArgumentNullException(nameof(unit));
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
    }

    // Unit properties
    public string Id => _unit.Id;
    public string DisplayName => _unit.DisplayName;
    public UnitRole Role => _unit.Role;
    public int Tier => _unit.Tier;
    public double Health => _unit.Health;
    public double Damage => _unit.Damage;
    public double AttacksPerSecond => _unit.AttacksPerSecond;
    public double Armor => _unit.Armor;
    public double Range => _unit.Range;
    public int WoodCost => _unit.WoodCost;
    public int GoldCost => _unit.GoldCost;
    public int PopulationCost => _unit.PopulationCost;
    public double ProductionTimeSeconds => _unit.ProductionTimeSeconds;

    // Calculated properties
    public double TotalCost => _unit.WoodCost + _unit.GoldCost;

    public double DPS
    {
        get
        {
            var metrics = _calculator.Calculate(_unit);
            return metrics.DamagePerSecond;
        }
    }

    public double DPSPerCost
    {
        get
        {
            var metrics = _calculator.Calculate(_unit);
            return metrics.DpsPerCost;
        }
    }

    public double EffectiveHealth
    {
        get
        {
            var metrics = _calculator.Calculate(_unit);
            return metrics.EffectiveHealth;
        }
    }

    // For filtering
    public UnitDefinition UnitDefinition => _unit;
}
