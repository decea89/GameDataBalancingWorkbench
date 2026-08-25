namespace BalanceForge.Infrastructure.Mapping;

using BalanceForge.Domain;
using BalanceForge.Infrastructure.Models;

/// <summary>
/// Simple mapper to convert between JSON models (Infrastructure) and Domain models.
/// Keeps the layers independent and allows schema evolution without affecting business logic.
/// </summary>
public static class UnitMapper
{
    /// <summary>
    /// Converts a JSON model to a Domain unit definition.
    /// </summary>
    public static UnitDefinition FromJson(UnitJsonModel json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (!Enum.TryParse<UnitRole>(json.Role, ignoreCase: true, out var role))
        {
            throw new InvalidOperationException($"Unknown role '{json.Role}' in unit '{json.Id}'.");
        }

        return new UnitDefinition
        {
            Id = json.Id,
            DisplayName = json.DisplayName,
            ImagePath = json.ImagePath,
            Role = role,
            Tier = json.Tier,
            Health = json.Health,
            Damage = json.Damage,
            AttacksPerSecond = json.AttacksPerSecond,
            Armor = json.Armor,
            Range = json.Range,
            WoodCost = json.WoodCost,
            GoldCost = json.GoldCost,
            PopulationCost = json.PopulationCost,
            ProductionTimeSeconds = json.ProductionTimeSeconds,
            AllowCostTierInversion = json.AllowCostTierInversion
        };
    }

    /// <summary>
    /// Converts a Domain unit definition to a JSON model.
    /// </summary>
    public static UnitJsonModel ToJson(UnitDefinition unit)
    {
        ArgumentNullException.ThrowIfNull(unit);

        return new UnitJsonModel
        {
            Id = unit.Id,
            DisplayName = unit.DisplayName,
            ImagePath = unit.ImagePath,
            Role = unit.Role.ToString(),
            Tier = unit.Tier,
            Health = unit.Health,
            Damage = unit.Damage,
            AttacksPerSecond = unit.AttacksPerSecond,
            Armor = unit.Armor,
            Range = unit.Range,
            WoodCost = unit.WoodCost,
            GoldCost = unit.GoldCost,
            PopulationCost = unit.PopulationCost,
            ProductionTimeSeconds = unit.ProductionTimeSeconds,
            AllowCostTierInversion = unit.AllowCostTierInversion
        };
    }

    /// <summary>
    /// Converts a collection of JSON models to Domain unit definitions.
    /// </summary>
    public static IEnumerable<UnitDefinition> FromJsonRoster(IEnumerable<UnitJsonModel> jsonUnits)
    {
        ArgumentNullException.ThrowIfNull(jsonUnits);
        return jsonUnits.Select(FromJson);
    }

    /// <summary>
    /// Converts a collection of Domain unit definitions to JSON models.
    /// </summary>
    public static IEnumerable<UnitJsonModel> ToJsonRoster(IEnumerable<UnitDefinition> domainUnits)
    {
        ArgumentNullException.ThrowIfNull(domainUnits);
        return domainUnits.Select(ToJson);
    }
}
