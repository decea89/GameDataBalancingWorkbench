namespace BalanceForge.Desktop.ViewModels;

using System.Collections.ObjectModel;
using BalanceForge.Application;
using BalanceForge.Domain;
using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>
/// View model for side-by-side unit comparison.
/// Displays base stats, derived metrics, and validation issues for two selected units.
/// </summary>
public partial class ComparisonViewModel : ObservableObject
{
    private readonly BalanceMetricsCalculator _metricsCalculator;
    private readonly UnitValidationService _validationService;

    [ObservableProperty]
    private RosterUnitViewModel? unitA;

    [ObservableProperty]
    private RosterUnitViewModel? unitB;

    [ObservableProperty]
    private ObservableCollection<ValidationIssue> combinedIssues = new();

    [ObservableProperty]
    private ObservableCollection<StatComparison> baseStats = new();

    [ObservableProperty]
    private ObservableCollection<MetricComparison> derivedMetrics = new();

    [ObservableProperty]
    private string relatedIssuesMessage = "No validation issues for the selected units.";

    [ObservableProperty]
    private bool hasComparison = false;

    [ObservableProperty]
    private string comparisonStatus = "Select 2 different units to compare.";

    public ComparisonViewModel(
        BalanceMetricsCalculator metricsCalculator,
        UnitValidationService validationService)
    {
        _metricsCalculator = metricsCalculator ?? throw new ArgumentNullException(nameof(metricsCalculator));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
    }

    /// <summary>
    /// Set the two units for comparison.
    /// </summary>
    public void SetComparison(RosterUnitViewModel? unitA, RosterUnitViewModel? unitB)
    {
        // Handle no selection
        if (unitA == null && unitB == null)
        {
            UnitA = null;
            UnitB = null;
            HasComparison = false;
            ComparisonStatus = "Select 2 different units to compare.";
            ClearComparisonData();
            return;
        }

        // Handle one unit selected
        if (unitA == null || unitB == null)
        {
            UnitA = unitA;
            UnitB = unitB;
            HasComparison = false;
            ComparisonStatus = "Select a second unit to compare.";
            ClearComparisonData();
            return;
        }

        // Handle identical selection
        if (unitA.Id == unitB.Id)
        {
            UnitA = unitA;
            UnitB = unitB;
            HasComparison = true;
            ComparisonStatus = $"Comparing {unitA.DisplayName} with itself.";
            RefreshComparisonData(unitA.UnitDefinition, unitB.UnitDefinition);
            return;
        }

        // Normal case: two different units
        UnitA = unitA;
        UnitB = unitB;
        HasComparison = true;
        ComparisonStatus = $"Comparing {unitA.DisplayName} vs {unitB.DisplayName}";
        RefreshComparisonData(unitA.UnitDefinition, unitB.UnitDefinition);
    }

    /// <summary>
    /// Update comparison when a unit is edited.
    /// </summary>
    public void RefreshComparison()
    {
        if (UnitA?.UnitDefinition != null && UnitB?.UnitDefinition != null)
        {
            RefreshComparisonData(UnitA.UnitDefinition, UnitB.UnitDefinition);
        }
    }

    private void RefreshComparisonData(UnitDefinition unitDefA, UnitDefinition unitDefB)
    {
        BaseStats = new ObservableCollection<StatComparison>(
            new[]
            {
                "Health", "Damage", "Atk/Sec", "Armor", "Range",
                "Wood Cost", "Gold Cost", "Pop Cost", "Prod Time"
            }
            .Select(GetStatComparison)
            .OfType<StatComparison>());

        DerivedMetrics = new ObservableCollection<MetricComparison>(
            new[] { "TotalCost", "DPS", "DPS/Cost", "Effective Health" }
                .Select(GetMetricComparison)
                .OfType<MetricComparison>());

        var issuesA = _validationService.Validate(unitDefA);
        var issuesB = _validationService.Validate(unitDefB);

        var combined = issuesA
            .Concat(issuesB)
            .DistinctBy(issue => (issue.RuleId, issue.UnitId, issue.Message))
            .OrderByDescending(issue => issue.Severity)
            .ThenBy(issue => issue.Message)
            .ToList();

        CombinedIssues.Clear();
        foreach (var issue in combined)
        {
            CombinedIssues.Add(issue);
        }

        RelatedIssuesMessage = combined.Count == 0
            ? "No validation issues for the selected units."
            : $"{combined.Count} validation issue{(combined.Count == 1 ? string.Empty : "s")} found.";
    }

    private void ClearComparisonData()
    {
        BaseStats.Clear();
        DerivedMetrics.Clear();
        CombinedIssues.Clear();
        RelatedIssuesMessage = "No validation issues for the selected units.";
    }

    /// <summary>
    /// Get metric comparison data between two units.
    /// </summary>
    public MetricComparison? GetMetricComparison(string metricName)
    {
        if (UnitA?.UnitDefinition == null || UnitB?.UnitDefinition == null)
            return null;

        var metricsA = _metricsCalculator.Calculate(UnitA.UnitDefinition);
        var metricsB = _metricsCalculator.Calculate(UnitB.UnitDefinition);

        return metricName switch
        {
            "TotalCost" => new MetricComparison(
                metricName,
                metricsA.TotalCost,
                metricsB.TotalCost,
                $"{metricsA.TotalCost}",
                $"{metricsB.TotalCost}"
            ),
            "DPS" => new MetricComparison(
                metricName,
                metricsA.DamagePerSecond,
                metricsB.DamagePerSecond,
                $"{metricsA.DamagePerSecond:F2}",
                $"{metricsB.DamagePerSecond:F2}"
            ),
            "DPS/Cost" => new MetricComparison(
                metricName,
                metricsA.DpsPerCost,
                metricsB.DpsPerCost,
                $"{metricsA.DpsPerCost:F4}",
                $"{metricsB.DpsPerCost:F4}"
            ),
            "Effective Health" => new MetricComparison(
                metricName,
                metricsA.EffectiveHealth,
                metricsB.EffectiveHealth,
                $"{metricsA.EffectiveHealth:F2}",
                $"{metricsB.EffectiveHealth:F2}"
            ),
            _ => null
        };
    }

    /// <summary>
    /// Get base stat comparison.
    /// </summary>
    public StatComparison? GetStatComparison(string statName)
    {
        if (UnitA?.UnitDefinition == null || UnitB?.UnitDefinition == null)
            return null;

        var unitA = UnitA.UnitDefinition;
        var unitB = UnitB.UnitDefinition;

        return statName switch
        {
            "Health" => new StatComparison(
                "Health",
                unitA.Health,
                unitB.Health,
                $"{unitA.Health}",
                $"{unitB.Health}"
            ),
            "Damage" => new StatComparison(
                "Damage",
                unitA.Damage,
                unitB.Damage,
                $"{unitA.Damage:F2}",
                $"{unitB.Damage:F2}"
            ),
            "Atk/Sec" => new StatComparison(
                "Atk/Sec",
                unitA.AttacksPerSecond,
                unitB.AttacksPerSecond,
                $"{unitA.AttacksPerSecond:F2}",
                $"{unitB.AttacksPerSecond:F2}"
            ),
            "Armor" => new StatComparison(
                "Armor",
                unitA.Armor,
                unitB.Armor,
                $"{unitA.Armor}",
                $"{unitB.Armor}"
            ),
            "Range" => new StatComparison(
                "Range",
                unitA.Range,
                unitB.Range,
                $"{unitA.Range}",
                $"{unitB.Range}"
            ),
            "Wood Cost" => new StatComparison(
                "Wood Cost",
                unitA.WoodCost,
                unitB.WoodCost,
                $"{unitA.WoodCost}",
                $"{unitB.WoodCost}"
            ),
            "Gold Cost" => new StatComparison(
                "Gold Cost",
                unitA.GoldCost,
                unitB.GoldCost,
                $"{unitA.GoldCost}",
                $"{unitB.GoldCost}"
            ),
            "Pop Cost" => new StatComparison(
                "Pop Cost",
                unitA.PopulationCost,
                unitB.PopulationCost,
                $"{unitA.PopulationCost}",
                $"{unitB.PopulationCost}"
            ),
            "Prod Time" => new StatComparison(
                "Prod Time",
                unitA.ProductionTimeSeconds,
                unitB.ProductionTimeSeconds,
                $"{unitA.ProductionTimeSeconds:F1}s",
                $"{unitB.ProductionTimeSeconds:F1}s"
            ),
            _ => null
        };
    }
}

/// <summary>
/// Represents a comparison between two metric values.
/// </summary>
public record MetricComparison(
    string Name,
    double ValueA,
    double ValueB,
    string DisplayValueA,
    string DisplayValueB)
{
    /// <summary>
    /// Indicates which unit has the higher value: -1 for A, 0 for equal, 1 for B.
    /// </summary>
    public int Winner
    {
        get
        {
            if (Math.Abs(ValueA - ValueB) < 0.0001) return 0;
            return ValueA > ValueB ? -1 : 1;
        }
    }

    public string WinnerLabel
    {
        get
        {
            return Winner switch
            {
                -1 => "A",
                0 => "=",
                _ => "B"
            };
        }
    }
}

/// <summary>
/// Represents a comparison between two base stat values.
/// </summary>
public record StatComparison(
    string Name,
    double ValueA,
    double ValueB,
    string DisplayValueA,
    string DisplayValueB)
{
    /// <summary>
    /// Indicates which unit has the higher value: -1 for A, 0 for equal, 1 for B.
    /// </summary>
    public int Winner
    {
        get
        {
            if (Math.Abs(ValueA - ValueB) < 0.0001) return 0;
            return ValueA > ValueB ? -1 : 1;
        }
    }

    public string WinnerLabel
    {
        get
        {
            return Winner switch
            {
                -1 => "A",
                0 => "=",
                _ => "B"
            };
        }
    }
}
