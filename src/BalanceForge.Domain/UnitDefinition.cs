namespace BalanceForge.Domain;

/// <summary>
/// Represents a unit definition with all balance-relevant attributes.
/// 
/// This is a mutable data model suitable for editing workflows. It holds no validation logic,
/// calculated metrics, or persistence concerns.
/// </summary>
public class UnitDefinition
{
    /// <summary>
    /// Unique identifier for the unit.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Display name shown to designers.
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// The unit's archetype role.
    /// </summary>
    public required UnitRole Role { get; set; }

    /// <summary>
    /// Tier ranking (typically 1–4). Used for balance comparisons.
    /// </summary>
    public required int Tier { get; set; }

    /// <summary>
    /// Health points. May be fractional for internal calculations.
    /// </summary>
    public required double Health { get; set; }

    /// <summary>
    /// Damage per attack. May be fractional.
    /// </summary>
    public required double Damage { get; set; }

    /// <summary>
    /// Number of attacks per second. May be fractional.
    /// </summary>
    public required double AttacksPerSecond { get; set; }

    /// <summary>
    /// Armor value, reducing damage taken.
    /// </summary>
    public required double Armor { get; set; }

    /// <summary>
    /// Attack range in game units.
    /// </summary>
    public required double Range { get; set; }

    /// <summary>
    /// Cost in wood (whole resource).
    /// </summary>
    public required int WoodCost { get; set; }

    /// <summary>
    /// Cost in gold (whole resource).
    /// </summary>
    public required int GoldCost { get; set; }

    /// <summary>
    /// Population slots consumed (whole resource).
    /// </summary>
    public required int PopulationCost { get; set; }

    /// <summary>
    /// Production time in seconds. May be fractional.
    /// </summary>
    public required double ProductionTimeSeconds { get; set; }
}
