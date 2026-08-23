namespace BalanceForge.Domain;

/// <summary>
/// Immutable result containing calculated balance metrics for a unit.
/// These are diagnostic signals, not objective balance truth.
/// </summary>
public record BalanceMetrics(
    double DamagePerSecond,
    int TotalCost,
    double DpsPerCost,
    double EffectiveHealth
);
