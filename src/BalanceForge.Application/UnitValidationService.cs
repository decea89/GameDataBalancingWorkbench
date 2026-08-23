namespace BalanceForge.Application;

using BalanceForge.Domain;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Pure validation service that checks units against both single-unit and cross-unit rules.
/// Returns all issues found; does not stop at the first issue.
/// </summary>
public class UnitValidationService
{
    private readonly BalanceMetricsCalculator _metricsCalculator = new();
    /// <summary>
    /// Validate a single unit and return all issues found.
    /// </summary>
    /// <param name="unit">The unit to validate (not mutated).</param>
    /// <returns>A collection of all validation issues found. Empty if unit is valid.</returns>
    public IEnumerable<ValidationIssue> Validate(UnitDefinition unit)
    {
        ArgumentNullException.ThrowIfNull(unit);

        var issues = new List<ValidationIssue>();

        // Rule 1: Check for negative values
        if (unit.Health < 0)
        {
            issues.Add(new ValidationIssue(
                RuleId: "NEGATIVE_HEALTH",
                Severity: ValidationSeverity.Error,
                UnitId: unit.Id,
                Message: $"Unit '{unit.DisplayName}' has negative health ({unit.Health}). Health must be zero or positive.",
                SuggestedAction: "Set health to a value >= 0."
            ));
        }

        if (unit.Damage < 0)
        {
            issues.Add(new ValidationIssue(
                RuleId: "NEGATIVE_DAMAGE",
                Severity: ValidationSeverity.Error,
                UnitId: unit.Id,
                Message: $"Unit '{unit.DisplayName}' has negative damage ({unit.Damage}). Damage must be zero or positive.",
                SuggestedAction: "Set damage to a value >= 0."
            ));
        }

        if (unit.WoodCost < 0)
        {
            issues.Add(new ValidationIssue(
                RuleId: "NEGATIVE_WOOD_COST",
                Severity: ValidationSeverity.Error,
                UnitId: unit.Id,
                Message: $"Unit '{unit.DisplayName}' has negative wood cost ({unit.WoodCost}). Cost must be zero or positive.",
                SuggestedAction: "Set wood cost to a value >= 0."
            ));
        }

        if (unit.GoldCost < 0)
        {
            issues.Add(new ValidationIssue(
                RuleId: "NEGATIVE_GOLD_COST",
                Severity: ValidationSeverity.Error,
                UnitId: unit.Id,
                Message: $"Unit '{unit.DisplayName}' has negative gold cost ({unit.GoldCost}). Cost must be zero or positive.",
                SuggestedAction: "Set gold cost to a value >= 0."
            ));
        }

        if (unit.ProductionTimeSeconds < 0)
        {
            issues.Add(new ValidationIssue(
                RuleId: "NEGATIVE_PRODUCTION_TIME",
                Severity: ValidationSeverity.Error,
                UnitId: unit.Id,
                Message: $"Unit '{unit.DisplayName}' has negative production time ({unit.ProductionTimeSeconds}). Production time must be zero or positive.",
                SuggestedAction: "Set production time to a value >= 0."
            ));
        }

        // Rule 2: Non-support units must have DPS > 0
        if (unit.Role != UnitRole.Support)
        {
            var dps = unit.Damage * unit.AttacksPerSecond;
            if (dps <= 0)
            {
                issues.Add(new ValidationIssue(
                    RuleId: "ZERO_DPS_NON_SUPPORT",
                    Severity: ValidationSeverity.Error,
                    UnitId: unit.Id,
                    Message: $"Unit '{unit.DisplayName}' (role: {unit.Role}) has zero DPS. Non-support units must have positive DPS.",
                    SuggestedAction: "Increase damage or attacks per second, or change the unit role to Support."
                ));
            }
        }

        // Rule 3: Tier must be between 1 and 4
        if (unit.Tier < 1 || unit.Tier > 4)
        {
            issues.Add(new ValidationIssue(
                RuleId: "INVALID_TIER",
                Severity: ValidationSeverity.Error,
                UnitId: unit.Id,
                Message: $"Unit '{unit.DisplayName}' has tier {unit.Tier}. Tier must be between 1 and 4.",
                SuggestedAction: "Set tier to a value between 1 and 4."
            ));
        }

        return issues;
    }

    /// <summary>
    /// Validate a roster of units against cross-unit rules.
    /// </summary>
    /// <param name="units">The units to validate together (not mutated).</param>
    /// <returns>A collection of all cross-unit validation issues found.</returns>
    public IEnumerable<ValidationIssue> ValidateRoster(IEnumerable<UnitDefinition> units)
    {
        ArgumentNullException.ThrowIfNull(units);

        var unitList = units.ToList();
        var issues = new List<ValidationIssue>();

        // Rule: Tier 2+ units cannot have lower total cost than Tier 1 units of the same role,
        // unless explicitly allowed via AllowCostTierInversion.
        var tier1ByRole = unitList
            .Where(u => u.Tier == 1)
            .GroupBy(u => u.Role)
            .ToDictionary(g => g.Key, g => g.Min(u => _metricsCalculator.Calculate(u).TotalCost));

        var tier2PlusUnits = unitList.Where(u => u.Tier >= 2);
        var processedHigherTierUnits = new HashSet<string>();

        foreach (var unit in tier2PlusUnits)
        {
            if (processedHigherTierUnits.Contains(unit.Id))
                continue;

            if (!tier1ByRole.TryGetValue(unit.Role, out var tier1MinCost))
                continue;

            var unitTotalCost = _metricsCalculator.Calculate(unit).TotalCost;

            if (unitTotalCost < tier1MinCost && !unit.AllowCostTierInversion)
            {
                issues.Add(new ValidationIssue(
                    RuleId: "TIER_COST_INVERSION",
                    Severity: ValidationSeverity.Warning,
                    UnitId: unit.Id,
                    Message: $"Unit '{unit.DisplayName}' (Tier {unit.Tier}, role: {unit.Role}) has total cost {unitTotalCost}, which is lower than the minimum Tier 1 {unit.Role} cost ({tier1MinCost}). Higher-tier units should generally cost more.",
                    SuggestedAction: "Increase cost or set AllowCostTierInversion to true if this inversion is intentional."
                ));

                processedHigherTierUnits.Add(unit.Id);
            }
        }

        return issues;
    }
}
