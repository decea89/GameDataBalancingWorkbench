namespace BalanceForge.Infrastructure.Models;

using System.Text.Json.Serialization;

/// <summary>
/// JSON serializable model for a single unit definition.
/// Kept separate from the domain model to allow independent evolution of JSON schema and business logic.
/// </summary>
public class UnitJsonModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("imagePath")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ImagePath { get; set; }

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("tier")]
    public int Tier { get; set; }

    [JsonPropertyName("health")]
    public double Health { get; set; }

    [JsonPropertyName("damage")]
    public double Damage { get; set; }

    [JsonPropertyName("attacksPerSecond")]
    public double AttacksPerSecond { get; set; }

    [JsonPropertyName("armor")]
    public double Armor { get; set; }

    [JsonPropertyName("range")]
    public double Range { get; set; }

    [JsonPropertyName("woodCost")]
    public int WoodCost { get; set; }

    [JsonPropertyName("goldCost")]
    public int GoldCost { get; set; }

    [JsonPropertyName("populationCost")]
    public int PopulationCost { get; set; }

    [JsonPropertyName("productionTimeSeconds")]
    public double ProductionTimeSeconds { get; set; }

    [JsonPropertyName("allowCostTierInversion")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool AllowCostTierInversion { get; set; }
}
