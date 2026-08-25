namespace BalanceForge.Infrastructure.Tests;

using BalanceForge.Domain;
using BalanceForge.Infrastructure.Mapping;
using BalanceForge.Infrastructure.Models;
using Xunit;

public class UnitMapperTests
{
    [Fact]
    public void FromJson_ConvertsJsonModelToDomainUnit()
    {
        // Arrange
        var json = new UnitJsonModel
        {
            Id = "knight",
            DisplayName = "Knight",
            ImagePath = "images/knight.png",
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
            ProductionTimeSeconds = 28,
            AllowCostTierInversion = true
        };

        // Act
        var unit = UnitMapper.FromJson(json);

        // Assert
        Assert.Equal("knight", unit.Id);
        Assert.Equal("Knight", unit.DisplayName);
        Assert.Equal("images/knight.png", unit.ImagePath);
        Assert.Equal(UnitRole.Cavalry, unit.Role);
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
        Assert.True(unit.AllowCostTierInversion);
    }

    [Fact]
    public void FromJson_ThrowsOnNullInput()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => UnitMapper.FromJson(null!));
    }

    [Fact]
    public void FromJson_ThrowsOnUnknownRole()
    {
        // Arrange
        var json = new UnitJsonModel
        {
            Id = "bad",
            DisplayName = "Bad",
            Role = "UnknownRole",
            Tier = 1,
            Health = 100,
            Damage = 10,
            AttacksPerSecond = 1,
            Armor = 0,
            Range = 1,
            WoodCost = 10,
            GoldCost = 10,
            PopulationCost = 1,
            ProductionTimeSeconds = 5
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => UnitMapper.FromJson(json));
        Assert.Contains("UnknownRole", ex.Message);
    }

    [Fact]
    public void ToJson_ConvertsDomainUnitToJsonModel()
    {
        // Arrange
        var unit = new UnitDefinition
        {
            Id = "archer",
            DisplayName = "Archer",
            ImagePath = "images/archer.png",
            Role = UnitRole.Ranged,
            Tier = 1,
            Health = 30d,
            Damage = 6d,
            AttacksPerSecond = 1.5,
            Armor = 0,
            Range = 5.0,
            WoodCost = 30,
            GoldCost = 10,
            PopulationCost = 1,
            ProductionTimeSeconds = 8d,
            AllowCostTierInversion = false
        };

        // Act
        var json = UnitMapper.ToJson(unit);

        // Assert
        Assert.Equal("archer", json.Id);
        Assert.Equal("Archer", json.DisplayName);
        Assert.Equal("images/archer.png", json.ImagePath);
        Assert.Equal("Ranged", json.Role);
        Assert.Equal(1, json.Tier);
        Assert.Equal(30d, json.Health);
        Assert.Equal(6d, json.Damage);
        Assert.Equal(1.5, json.AttacksPerSecond);
        Assert.Equal(0d, json.Armor);
        Assert.Equal(5.0, json.Range);
        Assert.Equal(30, json.WoodCost);
        Assert.Equal(10, json.GoldCost);
        Assert.Equal(1, json.PopulationCost);
        Assert.Equal(8d, json.ProductionTimeSeconds);
        Assert.False(json.AllowCostTierInversion);
    }

    [Fact]
    public void ToJson_ThrowsOnNullInput()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => UnitMapper.ToJson(null!));
    }

    [Fact]
    public void RoundTrip_PreservesAllProperties()
    {
        // Arrange
        var original = new UnitDefinition
        {
            Id = "test",
            DisplayName = "Test",
            ImagePath = "images/test.png",
            Role = UnitRole.Support,
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
        };

        // Act
        var json = UnitMapper.ToJson(original);
        var roundTripped = UnitMapper.FromJson(json);

        // Assert
        Assert.Equal(original.Id, roundTripped.Id);
        Assert.Equal(original.DisplayName, roundTripped.DisplayName);
        Assert.Equal(original.ImagePath, roundTripped.ImagePath);
        Assert.Equal(original.Role, roundTripped.Role);
        Assert.Equal(original.Tier, roundTripped.Tier);
        Assert.Equal(original.Health, roundTripped.Health);
        Assert.Equal(original.Damage, roundTripped.Damage);
        Assert.Equal(original.AttacksPerSecond, roundTripped.AttacksPerSecond);
        Assert.Equal(original.Armor, roundTripped.Armor);
        Assert.Equal(original.Range, roundTripped.Range);
        Assert.Equal(original.WoodCost, roundTripped.WoodCost);
        Assert.Equal(original.GoldCost, roundTripped.GoldCost);
        Assert.Equal(original.PopulationCost, roundTripped.PopulationCost);
        Assert.Equal(original.ProductionTimeSeconds, roundTripped.ProductionTimeSeconds);
        Assert.Equal(original.AllowCostTierInversion, roundTripped.AllowCostTierInversion);
    }

    [Fact]
    public void FromJsonRoster_ConvertsJsonRosterToDomainUnits()
    {
        // Arrange
        var json = new List<UnitJsonModel>
        {
            new()
            {
                Id = "unit1",
                DisplayName = "Unit 1",
                Role = "Infantry",
                Tier = 1,
                Health = 100d,
                Damage = 10d,
                AttacksPerSecond = 1d,
                Armor = 0d,
                Range = 1d,
                WoodCost = 10,
                GoldCost = 10,
                PopulationCost = 1,
                ProductionTimeSeconds = 5d
            },
            new()
            {
                Id = "unit2",
                DisplayName = "Unit 2",
                Role = "Ranged",
                Tier = 2,
                Health = 50d,
                Damage = 8d,
                AttacksPerSecond = 1.5d,
                Armor = 0d,
                Range = 5d,
                WoodCost = 20,
                GoldCost = 20,
                PopulationCost = 1,
                ProductionTimeSeconds = 8d
            }
        };

        // Act
        var units = UnitMapper.FromJsonRoster(json).ToList();

        // Assert
        Assert.Equal(2, units.Count);
        Assert.Equal("unit1", units[0].Id);
        Assert.Equal(UnitRole.Infantry, units[0].Role);
        Assert.Equal("unit2", units[1].Id);
        Assert.Equal(UnitRole.Ranged, units[1].Role);
    }

    [Fact]
    public void ToJsonRoster_ConvertsDomainUnitsToJsonRoster()
    {
        // Arrange
        var units = new List<UnitDefinition>
        {
            new()
            {
                Id = "unit1",
                DisplayName = "Unit 1",
                Role = UnitRole.Infantry,
                Tier = 1,
                Health = 100d,
                Damage = 10d,
                AttacksPerSecond = 1d,
                Armor = 0d,
                Range = 1d,
                WoodCost = 10,
                GoldCost = 10,
                PopulationCost = 1,
                ProductionTimeSeconds = 5d
            }
        };

        // Act
        var json = UnitMapper.ToJsonRoster(units).ToList();

        // Assert
        Assert.Single(json);
        Assert.Equal("unit1", json[0].Id);
        Assert.Equal("Infantry", json[0].Role);
    }

    [Fact]
    public void FromJsonRoster_ThrowsOnNullInput()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => UnitMapper.FromJsonRoster(null!));
    }

    [Fact]
    public void ToJsonRoster_ThrowsOnNullInput()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => UnitMapper.ToJsonRoster(null!));
    }

    [Theory]
    [InlineData("Infantry")]
    [InlineData("infantry")]
    [InlineData("INFANTRY")]
    public void FromJson_RoleParsingIsCaseInsensitive(string roleString)
    {
        // Arrange
        var json = new UnitJsonModel
        {
            Id = "test",
            DisplayName = "Test",
            Role = roleString,
            Tier = 1,
            Health = 100d,
            Damage = 10d,
            AttacksPerSecond = 1d,
            Armor = 0d,
            Range = 1d,
            WoodCost = 10,
            GoldCost = 10,
            PopulationCost = 1,
            ProductionTimeSeconds = 5d
        };

        // Act
        var unit = UnitMapper.FromJson(json);

        // Assert
        Assert.Equal(UnitRole.Infantry, unit.Role);
    }
}
