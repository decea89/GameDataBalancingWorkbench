namespace BalanceForge.Infrastructure.Models;

using System.Text.Json.Serialization;

/// <summary>
/// JSON serializable model for a roster of units.
/// Represents the root structure of units.json.
/// </summary>
public class RosterJsonModel
{
    [JsonPropertyName("units")]
    public List<UnitJsonModel> Units { get; set; } = new();
}
