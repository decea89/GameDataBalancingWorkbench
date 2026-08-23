namespace BalanceForge.Application;

using BalanceForge.Domain;

/// <summary>
/// Pure, stateless calculator for unit balance metrics.
/// Accepts a unit definition and returns calculated diagnostic metrics.
/// No side effects, no external dependencies.
/// </summary>
public class BalanceMetricsCalculator
{
    /// <summary>
    /// Calculate balance metrics for a unit.
    /// </summary>
    /// <param name="unit">The unit definition (not mutated).</param>
    /// <returns>An immutable BalanceMetrics result.</returns>
    public BalanceMetrics Calculate(UnitDefinition unit)
    {
        ArgumentNullException.ThrowIfNull(unit);

        var damagePerSecond = unit.Damage * unit.AttacksPerSecond;
        var totalCost = unit.WoodCost + unit.GoldCost;
        var dpsPerCost = totalCost == 0 ? 0.0 : damagePerSecond / totalCost;
        var effectiveHealth = unit.Health * (1.0 + unit.Armor * 0.1);

        return new BalanceMetrics(
            damagePerSecond,
            totalCost,
            dpsPerCost,
            effectiveHealth
        );
    }
}
