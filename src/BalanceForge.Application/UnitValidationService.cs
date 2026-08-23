namespace BalanceForge.Application;

using BalanceForge.Domain;
using System.Collections.Generic;

/// <summary>
/// Pure validation service that checks a single UnitDefinition against basic rules.
/// Returns all issues found; does not stop at the first issue.
/// </summary>
public class UnitValidationService
{
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
}
