namespace BalanceForge.Desktop.ViewModels;

using System.IO;
using BalanceForge.Application;
using BalanceForge.Domain;
using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>
/// View model wrapper for a unit definition with calculated metrics.
/// Used to expose both domain model and calculated values for UI binding.
/// </summary>
public class RosterUnitViewModel : ObservableObject
{
    private readonly UnitDefinition _unit;
    private readonly BalanceMetricsCalculator _calculator;
    private readonly string? _rosterDirectory;

    public RosterUnitViewModel(
        UnitDefinition unit,
        BalanceMetricsCalculator calculator,
        string? rosterDirectory = null)
    {
        _unit = unit ?? throw new ArgumentNullException(nameof(unit));
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        _rosterDirectory = rosterDirectory;
    }

    // Unit properties
    public string Id => _unit.Id;
    public string DisplayName => _unit.DisplayName;
    public string UnitInitial => string.IsNullOrWhiteSpace(DisplayName)
        ? "?"
        : DisplayName[..1].ToUpperInvariant();
    public string? ImageSourcePath
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_unit.ImagePath))
            {
                return null;
            }

            var resolvedPath = Path.IsPathRooted(_unit.ImagePath)
                ? _unit.ImagePath
                : Path.Combine(_rosterDirectory ?? string.Empty, _unit.ImagePath);

            return File.Exists(resolvedPath) ? Path.GetFullPath(resolvedPath) : null;
        }
    }
    public bool HasImage => ImageSourcePath != null;
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

    /// <summary>
    /// Notifies the UI that the underlying unit and its calculated metrics changed.
    /// </summary>
    public void Refresh()
    {
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(UnitInitial));
        OnPropertyChanged(nameof(ImageSourcePath));
        OnPropertyChanged(nameof(HasImage));
        OnPropertyChanged(nameof(Role));
        OnPropertyChanged(nameof(Tier));
        OnPropertyChanged(nameof(Health));
        OnPropertyChanged(nameof(Damage));
        OnPropertyChanged(nameof(AttacksPerSecond));
        OnPropertyChanged(nameof(Armor));
        OnPropertyChanged(nameof(Range));
        OnPropertyChanged(nameof(WoodCost));
        OnPropertyChanged(nameof(GoldCost));
        OnPropertyChanged(nameof(PopulationCost));
        OnPropertyChanged(nameof(ProductionTimeSeconds));
        OnPropertyChanged(nameof(TotalCost));
        OnPropertyChanged(nameof(DPS));
        OnPropertyChanged(nameof(DPSPerCost));
        OnPropertyChanged(nameof(EffectiveHealth));
    }
}
