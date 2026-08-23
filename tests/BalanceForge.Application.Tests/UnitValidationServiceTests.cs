namespace BalanceForge.Application.Tests;

using BalanceForge.Domain;
using Xunit;

public class UnitValidationServiceTests
{
    private readonly UnitValidationService _validator = new();

    private UnitDefinition CreateValidUnit(string id = "test-unit")
    {
        return new UnitDefinition
        {
            Id = id,
            DisplayName = "Test Unit",
            Role = UnitRole.Infantry,
            Tier = 1,
            Health = 100,
            Damage = 10,
            AttacksPerSecond = 1,
            Armor = 0,
            Range = 1,
            WoodCost = 50,
            GoldCost = 50,
            PopulationCost = 1,
            ProductionTimeSeconds = 5
        };
    }

    [Fact]
    public void Validate_ValidUnit_ReturnsNoIssues()
    {
        // Arrange
        var unit = CreateValidUnit();

        // Act
        var issues = _validator.Validate(unit);

        // Assert
        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_NegativeHealth_ReturnsError()
    {
        // Arrange
        var unit = CreateValidUnit();
        unit.Health = -10;

        // Act
        var issues = _validator.Validate(unit);

        // Assert
        var issue = Assert.Single(issues);
        Assert.Equal("NEGATIVE_HEALTH", issue.RuleId);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Contains("health", issue.Message, StringComparison.OrdinalIgnoreCase);
        Assert.NotEmpty(issue.SuggestedAction);
    }

    [Fact]
    public void Validate_NegativeDamage_ReturnsError()
    {
        // Arrange
        var unit = CreateValidUnit();
        unit.Damage = -5;
        unit.Role = UnitRole.Support; // Support can have zero DPS, so only negative damage triggers

        // Act
        var issues = _validator.Validate(unit);

        // Assert
        var issue = Assert.Single(issues);
        Assert.Equal("NEGATIVE_DAMAGE", issue.RuleId);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
    }

    [Fact]
    public void Validate_NegativeWoodCost_ReturnsError()
    {
        // Arrange
        var unit = CreateValidUnit();
        unit.WoodCost = -20;

        // Act
        var issues = _validator.Validate(unit);

        // Assert
        var issue = Assert.Single(issues);
        Assert.Equal("NEGATIVE_WOOD_COST", issue.RuleId);
    }

    [Fact]
    public void Validate_NegativeGoldCost_ReturnsError()
    {
        // Arrange
        var unit = CreateValidUnit();
        unit.GoldCost = -30;

        // Act
        var issues = _validator.Validate(unit);

        // Assert
        var issue = Assert.Single(issues);
        Assert.Equal("NEGATIVE_GOLD_COST", issue.RuleId);
    }

    [Fact]
    public void Validate_NegativeProductionTime_ReturnsError()
    {
        // Arrange
        var unit = CreateValidUnit();
        unit.ProductionTimeSeconds = -2;

        // Act
        var issues = _validator.Validate(unit);

        // Assert
        var issue = Assert.Single(issues);
        Assert.Equal("NEGATIVE_PRODUCTION_TIME", issue.RuleId);
    }

    [Fact]
    public void Validate_MultipleNegativeValues_ReturnsMultipleIssues()
    {
        // Arrange
        var unit = CreateValidUnit();
        unit.Role = UnitRole.Support; // Avoid zero DPS rule
        unit.Health = -10;
        unit.Damage = -5;
        unit.WoodCost = -20;

        // Act
        var issues = _validator.Validate(unit).ToList();

        // Assert
        Assert.Equal(3, issues.Count);
        Assert.Contains(issues, i => i.RuleId == "NEGATIVE_HEALTH");
        Assert.Contains(issues, i => i.RuleId == "NEGATIVE_DAMAGE");
        Assert.Contains(issues, i => i.RuleId == "NEGATIVE_WOOD_COST");
    }

    [Fact]
    public void Validate_NonSupportWithZeroDps_ReturnsError()
    {
        // Arrange
        var unit = CreateValidUnit();
        unit.Role = UnitRole.Ranged;
        unit.Damage = 0;
        unit.AttacksPerSecond = 1;

        // Act
        var issues = _validator.Validate(unit);

        // Assert
        var issue = Assert.Single(issues);
        Assert.Equal("ZERO_DPS_NON_SUPPORT", issue.RuleId);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Contains("DPS", issue.Message);
    }

    [Fact]
    public void Validate_NonSupportWithZeroAttacksPerSecond_ReturnsError()
    {
        // Arrange
        var unit = CreateValidUnit();
        unit.Role = UnitRole.Infantry;
        unit.Damage = 10;
        unit.AttacksPerSecond = 0;

        // Act
        var issues = _validator.Validate(unit);

        // Assert
        var issue = Assert.Single(issues);
        Assert.Equal("ZERO_DPS_NON_SUPPORT", issue.RuleId);
    }

    [Fact]
    public void Validate_SupportWithZeroDps_ReturnsNoIssue()
    {
        // Arrange
        var unit = CreateValidUnit();
        unit.Role = UnitRole.Support;
        unit.Damage = 0;
        unit.AttacksPerSecond = 1;

        // Act
        var issues = _validator.Validate(unit);

        // Assert
        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_InvalidTierBelowRange_ReturnsError()
    {
        // Arrange
        var unit = CreateValidUnit();
        unit.Tier = 0;

        // Act
        var issues = _validator.Validate(unit);

        // Assert
        var issue = Assert.Single(issues);
        Assert.Equal("INVALID_TIER", issue.RuleId);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Contains("tier", issue.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_InvalidTierAboveRange_ReturnsError()
    {
        // Arrange
        var unit = CreateValidUnit();
        unit.Tier = 5;

        // Act
        var issues = _validator.Validate(unit);

        // Assert
        var issue = Assert.Single(issues);
        Assert.Equal("INVALID_TIER", issue.RuleId);
    }

    [Fact]
    public void Validate_ValidTierBoundaries_ReturnsNoIssue()
    {
        // Arrange & Act & Assert
        for (int tier = 1; tier <= 4; tier++)
        {
            var unit = CreateValidUnit();
            unit.Tier = tier;
            var issues = _validator.Validate(unit);
            Assert.Empty(issues);
        }
    }

    [Fact]
    public void Validate_UnitIdPresentInIssues()
    {
        // Arrange
        var unit = CreateValidUnit("special-id-123");
        unit.Health = -10;

        // Act
        var issues = _validator.Validate(unit);

        // Assert
        var issue = Assert.Single(issues);
        Assert.Equal("special-id-123", issue.UnitId);
    }

    [Fact]
    public void Validate_MessageContainsUnitDisplayName()
    {
        // Arrange
        var unit = CreateValidUnit();
        unit.DisplayName = "Dragon Slayer";
        unit.Health = -5;

        // Act
        var issues = _validator.Validate(unit);

        // Assert
        var issue = Assert.Single(issues);
        Assert.Contains("Dragon Slayer", issue.Message);
    }

    [Fact]
    public void Validate_ThrowsOnNullUnit()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _validator.Validate(null!));
    }
}
