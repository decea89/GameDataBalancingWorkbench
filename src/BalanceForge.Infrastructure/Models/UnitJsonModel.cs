namespace BalanceForge.Infrastructure.Models;

using System.Text.Json.Serialization;

/// <summary>
/// JSON serializable model for a single unit definition.
/// Kept separate from the domain model to allow independent evolution of JSON schema and business logic.
/// </summary>
public class UnitJsonModel
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("displayName")]
    public required string DisplayName { get; set; }

    [JsonPropertyName("role")]
    public required string Role { get; set; }

    [JsonPropertyName("tier")]
    public required int Tier { get; set; }

    [JsonPropertyName("health")]
    public required double Health { get; set; }

    [JsonPropertyName("damage")]
    public required double Damage { get; set; }

    [JsonPropertyName("attacksPerSecond")]
    public required double AttacksPerSecond { get; set; }

    [JsonPropertyName("armor")]
    public required double Armor { get; set; }

    [JsonPropertyName("range")]
    public required double Range { get; set; }

    [JsonPropertyName("woodCost")]
    public required int WoodCost { get; set; }

    [JsonPropertyName("goldCost")]
    public required int GoldCost { get; set; }

    [JsonPropertyName("populationCost")]
    public required int PopulationCost { get; set; }

    [JsonPropertyName("productionTimeSeconds")]
    public required double ProductionTimeSeconds { get; set; }

    [JsonPropertyName("allowCostTierInversion")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool AllowCostTierInversion { get; set; }
}
