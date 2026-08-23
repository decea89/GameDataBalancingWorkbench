namespace BalanceForge.Infrastructure.Tests;

using System.Text.Json;
using BalanceForge.Infrastructure.Models;
using Xunit;

public class RosterJsonModelTests
{
    [Fact]
    public void RosterJsonModel_SerializesToCamelCase()
    {
        // Arrange
        var roster = new RosterJsonModel
        {
            Units = new List<UnitJsonModel>
            {
                new()
                {
                    Id = "knight",
                    DisplayName = "Knight",
                    Role = "Cavalry",
                    Tier = 2,
                    Health = 180d,
                    Damage = 18d,
                    AttacksPerSecond = 1.1,
                    Armor = 4d,
                    Range = 1.5,
                    WoodCost = 0,
                    GoldCost = 90,
                    PopulationCost = 2,
                    ProductionTimeSeconds = 28d,
                    AllowCostTierInversion = false
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(roster, new JsonSerializerOptions { WriteIndented = true });

        // Assert
        Assert.Contains("\"id\":", json);
        Assert.Contains("\"displayName\":", json);
        Assert.Contains("\"role\":", json);
        Assert.Contains("\"tier\":", json);
        Assert.Contains("\"health\":", json);
        Assert.Contains("\"damage\":", json);
        Assert.Contains("\"attacksPerSecond\":", json);
        Assert.Contains("\"armor\":", json);
        Assert.Contains("\"range\":", json);
        Assert.Contains("\"woodCost\":", json);
        Assert.Contains("\"goldCost\":", json);
        Assert.Contains("\"populationCost\":", json);
        Assert.Contains("\"productionTimeSeconds\":", json);
        // AllowCostTierInversion should be omitted when false (default)
        Assert.DoesNotContain("allowCostTierInversion", json);
    }

    [Fact]
    public void RosterJsonModel_DeserializesFromCamelCase()
    {
        // Arrange
        var json = """
            {
              "units": [
                {
                  "id": "knight",
                  "displayName": "Knight",
                  "role": "Cavalry",
                  "tier": 2,
                  "health": 180,
                  "damage": 18,
                  "attacksPerSecond": 1.1,
                  "armor": 4,
                  "range": 1.5,
                  "woodCost": 0,
                  "goldCost": 90,
                  "populationCost": 2,
                  "productionTimeSeconds": 28
                }
              ]
            }
            """;

        // Act
        var roster = JsonSerializer.Deserialize<RosterJsonModel>(json);

        // Assert
        Assert.NotNull(roster);
        Assert.Single(roster.Units);

        var unit = roster.Units[0];
        Assert.Equal("knight", unit.Id);
        Assert.Equal("Knight", unit.DisplayName);
        Assert.Equal("Cavalry", unit.Role);
        Assert.Equal(2, unit.Tier);
        Assert.Equal(180, unit.Health);
        Assert.Equal(18, unit.Damage);
        Assert.Equal(1.1, unit.AttacksPerSecond);
        Assert.Equal(4, unit.Armor);
        Assert.Equal(1.5, unit.Range);
        Assert.Equal(0, unit.WoodCost);
        Assert.Equal(90, unit.GoldCost);
        Assert.Equal(2, unit.PopulationCost);
        Assert.Equal(28, unit.ProductionTimeSeconds);
        Assert.False(unit.AllowCostTierInversion); // Should default to false
    }

    [Fact]
    public void RosterJsonModel_DeserializesAllowCostTierInversionWhenTrue()
    {
        // Arrange
        var json = """
            {
              "units": [
                {
                  "id": "exception-unit",
                  "displayName": "Exception",
                  "role": "Infantry",
                  "tier": 2,
                  "health": 100,
                  "damage": 10,
                  "attacksPerSecond": 1.0,
                  "armor": 0,
                  "range": 1.0,
                  "woodCost": 50,
                  "goldCost": 50,
                  "populationCost": 1,
                  "productionTimeSeconds": 10,
                  "allowCostTierInversion": true
                }
              ]
            }
            """;

        // Act
        var roster = JsonSerializer.Deserialize<RosterJsonModel>(json);

        // Assert
        Assert.NotNull(roster);
        var unit = roster.Units[0];
        Assert.True(unit.AllowCostTierInversion);
    }

    [Fact]
    public void RosterJsonModel_SerializesMultipleUnits()
    {
        // Arrange
        var roster = new RosterJsonModel
        {
            Units = new List<UnitJsonModel>
            {
                new()
                {
                    Id = "footman",
                    DisplayName = "Footman",
                    Role = "Infantry",
                    Tier = 1,
                    Health = 50d,
                    Damage = 8d,
                    AttacksPerSecond = 1.0,
                    Armor = 1,
                    Range = 1.0,
                    WoodCost = 20,
                    GoldCost = 20,
                    PopulationCost = 1,
                    ProductionTimeSeconds = 5
                },
                new()
                {
                    Id = "archer",
                    DisplayName = "Archer",
                    Role = "Ranged",
                    Tier = 1,
                    Health = 30d,
                    Damage = 6d,
                    AttacksPerSecond = 1.5,
                    Armor = 0d,
                    Range = 5.0,
                    WoodCost = 30,
                    GoldCost = 10,
                    PopulationCost = 1,
                    ProductionTimeSeconds = 8
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(roster);

        // Assert
        var deserialized = JsonSerializer.Deserialize<RosterJsonModel>(json);
        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized.Units.Count);
        Assert.Equal("footman", deserialized.Units[0].Id);
        Assert.Equal("archer", deserialized.Units[1].Id);
    }

    [Fact]
    public void RosterJsonModel_DeserializesEmptyRoster()
    {
        // Arrange
        var json = """
            {
              "units": []
            }
            """;

        // Act
        var roster = JsonSerializer.Deserialize<RosterJsonModel>(json);

        // Assert
        Assert.NotNull(roster);
        Assert.Empty(roster.Units);
    }

    [Fact]
    public void RosterJsonModel_RoundTripPreservesAllProperties()
    {
        // Arrange
        var original = new RosterJsonModel
        {
            Units = new List<UnitJsonModel>
            {
                new()
                {
                    Id = "test",
                    DisplayName = "Test Unit",
                    Role = "Support",
                    Tier = 3,
                    Health = 120d,
                    Damage = 5d,
                    AttacksPerSecond = 0.8,
                    Armor = 2d,
                    Range = 2.5,
                    WoodCost = 60,
                    GoldCost = 80,
                    PopulationCost = 3,
                    ProductionTimeSeconds = 40d,
                    AllowCostTierInversion = true
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<RosterJsonModel>(json);

        // Assert
        Assert.NotNull(deserialized);
        var unit = deserialized.Units[0];
        Assert.Equal("test", unit.Id);
        Assert.Equal("Test Unit", unit.DisplayName);
        Assert.Equal("Support", unit.Role);
        Assert.Equal(3, unit.Tier);
        Assert.Equal(120, unit.Health);
        Assert.Equal(5, unit.Damage);
        Assert.Equal(0.8, unit.AttacksPerSecond);
        Assert.Equal(2, unit.Armor);
        Assert.Equal(2.5, unit.Range);
        Assert.Equal(60, unit.WoodCost);
        Assert.Equal(80, unit.GoldCost);
        Assert.Equal(3, unit.PopulationCost);
        Assert.Equal(40, unit.ProductionTimeSeconds);
        Assert.True(unit.AllowCostTierInversion);
    }
}
