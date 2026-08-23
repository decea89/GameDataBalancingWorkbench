namespace BalanceForge.Application.Tests;

using BalanceForge.Domain;
using Xunit;

public class BalanceMetricsCalculatorTests
{
    private readonly BalanceMetricsCalculator _calculator = new();

    [Fact]
    public void Calculate_NormalValues_ReturnsCorrectMetrics()
    {
        // Arrange
        var unit = new UnitDefinition
        {
            Id = "test-unit",
            DisplayName = "Test Unit",
            Role = UnitRole.Infantry,
            Tier = 1,
            Health = 100,
            Damage = 10,
            AttacksPerSecond = 2,
            Armor = 5,
            Range = 1,
            WoodCost = 50,
            GoldCost = 50,
            PopulationCost = 1,
            ProductionTimeSeconds = 5
        };

        // Act
        var metrics = _calculator.Calculate(unit);

        // Assert
        Assert.Equal(20, metrics.DamagePerSecond); // 10 * 2
        Assert.Equal(100, metrics.TotalCost);      // 50 + 50
        Assert.Equal(0.2, metrics.DpsPerCost);     // 20 / 100
        Assert.Equal(150, metrics.EffectiveHealth); // 100 * (1 + 5 * 0.1) = 100 * 1.5
    }

    [Fact]
    public void Calculate_FractionalAttacksPerSecond_ReturnsCorrectDps()
    {
        // Arrange
        var unit = new UnitDefinition
        {
            Id = "archer",
            DisplayName = "Archer",
            Role = UnitRole.Ranged,
            Tier = 1,
            Health = 30,
            Damage = 12,
            AttacksPerSecond = 1.5, // Fractional
            Armor = 0,
            Range = 5,
            WoodCost = 40,
            GoldCost = 20,
            PopulationCost = 1,
            ProductionTimeSeconds = 4
        };

        // Act
        var metrics = _calculator.Calculate(unit);

        // Assert
        Assert.Equal(18, metrics.DamagePerSecond); // 12 * 1.5
        Assert.Equal(60, metrics.TotalCost);       // 40 + 20
        Assert.Equal(0.3, metrics.DpsPerCost);     // 18 / 60
    }

    [Fact]
    public void Calculate_ZeroTotalCost_DpsPerCostIsZero()
    {
        // Arrange
        var unit = new UnitDefinition
        {
            Id = "free-unit",
            DisplayName = "Free Unit",
            Role = UnitRole.Support,
            Tier = 1,
            Health = 50,
            Damage = 5,
            AttacksPerSecond = 1,
            Armor = 0,
            Range = 1,
            WoodCost = 0,
            GoldCost = 0,
            PopulationCost = 1,
            ProductionTimeSeconds = 0
        };

        // Act
        var metrics = _calculator.Calculate(unit);

        // Assert
        Assert.Equal(5, metrics.DamagePerSecond);  // 5 * 1
        Assert.Equal(0, metrics.TotalCost);        // 0 + 0
        Assert.Equal(0, metrics.DpsPerCost);       // Guard against division by zero
    }

    [Fact]
    public void Calculate_ZeroDamage_DpsIsZero()
    {
        // Arrange
        var unit = new UnitDefinition
        {
            Id = "no-damage",
            DisplayName = "Non-Combat Unit",
            Role = UnitRole.Support,
            Tier = 1,
            Health = 100,
            Damage = 0,
            AttacksPerSecond = 2,
            Armor = 2,
            Range = 1,
            WoodCost = 30,
            GoldCost = 30,
            PopulationCost = 1,
            ProductionTimeSeconds = 3
        };

        // Act
        var metrics = _calculator.Calculate(unit);

        // Assert
        Assert.Equal(0, metrics.DamagePerSecond); // 0 * 2
        Assert.Equal(60, metrics.TotalCost);      // 30 + 30
        Assert.Equal(0, metrics.DpsPerCost);      // 0 / 60
    }

    [Fact]
    public void Calculate_ZeroArmor_EffectiveHealthEqualsHealth()
    {
        // Arrange
        var unit = new UnitDefinition
        {
            Id = "no-armor",
            DisplayName = "Squishy",
            Role = UnitRole.Ranged,
            Tier = 1,
            Health = 40,
            Damage = 8,
            AttacksPerSecond = 1,
            Armor = 0, // No armor
            Range = 4,
            WoodCost = 35,
            GoldCost = 25,
            PopulationCost = 1,
            ProductionTimeSeconds = 3
        };

        // Act
        var metrics = _calculator.Calculate(unit);

        // Assert
        Assert.Equal(40, metrics.EffectiveHealth); // 40 * (1 + 0 * 0.1) = 40 * 1
    }

    [Fact]
    public void Calculate_NonZeroArmor_EffectiveHealthIncreases()
    {
        // Arrange
        var unit = new UnitDefinition
        {
            Id = "armored-unit",
            DisplayName = "Knight",
            Role = UnitRole.Cavalry,
            Tier = 2,
            Health = 120,
            Damage = 15,
            AttacksPerSecond = 1,
            Armor = 10, // Significant armor
            Range = 1,
            WoodCost = 60,
            GoldCost = 40,
            PopulationCost = 2,
            ProductionTimeSeconds = 6
        };

        // Act
        var metrics = _calculator.Calculate(unit);

        // Assert
        var expectedEffectiveHealth = 120 * (1.0 + 10 * 0.1);
        Assert.Equal(expectedEffectiveHealth, metrics.EffectiveHealth); // 120 * 2.0 = 240
        Assert.Equal(240, metrics.EffectiveHealth);
    }

    [Fact]
    public void Calculate_ThrowsOnNullUnit()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _calculator.Calculate(null!));
    }
}
