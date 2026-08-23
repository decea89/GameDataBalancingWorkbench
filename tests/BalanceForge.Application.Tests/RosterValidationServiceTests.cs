namespace BalanceForge.Application.Tests;

using BalanceForge.Domain;
using Xunit;

public class RosterValidationServiceTests
{
    private readonly UnitValidationService _validator = new();

    private UnitDefinition CreateUnit(string id, int tier, UnitRole role, int woodCost, int goldCost)
    {
        return new UnitDefinition
        {
            Id = id,
            DisplayName = $"Unit-{id}",
            Role = role,
            Tier = tier,
            Health = 100,
            Damage = 10,
            AttacksPerSecond = 1,
            Armor = 0,
            Range = 1,
            WoodCost = woodCost,
            GoldCost = goldCost,
            PopulationCost = 1,
            ProductionTimeSeconds = 5,
            AllowCostTierInversion = false
        };
    }

    [Fact]
    public void ValidateRoster_ValidTier1Units_ReturnsNoIssues()
    {
        // Arrange
        var units = new[]
        {
            CreateUnit("t1-infantry-1", 1, UnitRole.Infantry, 50, 50),
            CreateUnit("t1-infantry-2", 1, UnitRole.Infantry, 60, 60),
            CreateUnit("t1-ranged-1", 1, UnitRole.Ranged, 40, 40)
        };

        // Act
        var issues = _validator.ValidateRoster(units);

        // Assert
        Assert.Empty(issues);
    }

    [Fact]
    public void ValidateRoster_Tier2WithHigherCost_ReturnsNoIssue()
    {
        // Arrange
        var units = new[]
        {
            CreateUnit("t1-infantry", 1, UnitRole.Infantry, 50, 50),  // Total: 100
            CreateUnit("t2-infantry", 2, UnitRole.Infantry, 75, 75)   // Total: 150 (higher)
        };

        // Act
        var issues = _validator.ValidateRoster(units);

        // Assert
        Assert.Empty(issues);
    }

    [Fact]
    public void ValidateRoster_Tier2WithLowerCost_ReturnsWarning()
    {
        // Arrange
        var units = new[]
        {
            CreateUnit("t1-infantry", 1, UnitRole.Infantry, 100, 100), // Total: 200
            CreateUnit("t2-infantry", 2, UnitRole.Infantry, 40, 40)    // Total: 80 (lower!)
        };

        // Act
        var issues = _validator.ValidateRoster(units).ToList();

        // Assert
        var issue = Assert.Single(issues);
        Assert.Equal("TIER_COST_INVERSION", issue.RuleId);
        Assert.Equal(ValidationSeverity.Warning, issue.Severity);
        Assert.Contains("t2-infantry", issue.UnitId);
        Assert.Contains("lower", issue.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateRoster_DifferentRolesDoNotConflict()
    {
        // Arrange
        var units = new[]
        {
            CreateUnit("t1-infantry", 1, UnitRole.Infantry, 100, 100),
            CreateUnit("t2-ranged", 2, UnitRole.Ranged, 50, 50)  // Different role, so no conflict
        };

        // Act
        var issues = _validator.ValidateRoster(units);

        // Assert
        Assert.Empty(issues);
    }

    [Fact]
    public void ValidateRoster_AllowCostTierInversionSuppressesIssue()
    {
        // Arrange
        var units = new[]
        {
            CreateUnit("t1-infantry", 1, UnitRole.Infantry, 100, 100), // Total: 200
            CreateUnit("t2-infantry", 2, UnitRole.Infantry, 40, 40)    // Total: 80 (lower, but allowed)
        };

        // Mark the Tier 2 unit as allowed to invert
        units[1].AllowCostTierInversion = true;

        // Act
        var issues = _validator.ValidateRoster(units);

        // Assert
        Assert.Empty(issues);
    }

    [Fact]
    public void ValidateRoster_MultipleTier1ComparatorsUsesMinimum()
    {
        // Arrange
        var units = new[]
        {
            CreateUnit("t1-infantry-1", 1, UnitRole.Infantry, 100, 100), // Total: 200
            CreateUnit("t1-infantry-2", 1, UnitRole.Infantry, 50, 50),   // Total: 100 (minimum)
            CreateUnit("t2-infantry", 2, UnitRole.Infantry, 80, 80)      // Total: 160
        };

        // Act
        var issues = _validator.ValidateRoster(units);

        // Assert
        // Tier 2 unit cost (160) is higher than minimum Tier 1 cost (100), so no issue
        Assert.Empty(issues);
    }

    [Fact]
    public void ValidateRoster_MultipleTier1ComparatorsIssueUsesMinimum()
    {
        // Arrange
        var units = new[]
        {
            CreateUnit("t1-infantry-1", 1, UnitRole.Infantry, 100, 100), // Total: 200
            CreateUnit("t1-infantry-2", 1, UnitRole.Infantry, 50, 50),   // Total: 100 (minimum)
            CreateUnit("t2-infantry", 2, UnitRole.Infantry, 60, 60)      // Total: 120 (still higher than min)
        };

        // Act
        var issues = _validator.ValidateRoster(units);

        // Assert
        Assert.Empty(issues);
    }

    [Fact]
    public void ValidateRoster_MultipleTier1ComparatorsIssueIfBelowMinimum()
    {
        // Arrange
        var units = new[]
        {
            CreateUnit("t1-infantry-1", 1, UnitRole.Infantry, 100, 100), // Total: 200
            CreateUnit("t1-infantry-2", 1, UnitRole.Infantry, 60, 60),   // Total: 120 (minimum)
            CreateUnit("t2-infantry", 2, UnitRole.Infantry, 40, 40)      // Total: 80 (below minimum!)
        };

        // Act
        var issues = _validator.ValidateRoster(units).ToList();

        // Assert
        var issue = Assert.Single(issues);
        Assert.Equal("TIER_COST_INVERSION", issue.RuleId);
        Assert.Contains("120", issue.Message); // Should mention the minimum Tier 1 cost
    }

    [Fact]
    public void ValidateRoster_Tier3And4WithLowerCost_ReturnsWarning()
    {
        // Arrange
        var units = new[]
        {
            CreateUnit("t1-infantry", 1, UnitRole.Infantry, 100, 100),  // Total: 200
            CreateUnit("t3-infantry", 3, UnitRole.Infantry, 50, 50),    // Total: 100 (lower!)
            CreateUnit("t4-infantry", 4, UnitRole.Infantry, 40, 40)     // Total: 80 (even lower!)
        };

        // Act
        var issues = _validator.ValidateRoster(units).ToList();

        // Assert
        Assert.Equal(2, issues.Count); // Both Tier 3 and Tier 4 should have issues
        Assert.All(issues, i => Assert.Equal("TIER_COST_INVERSION", i.RuleId));
    }

    [Fact]
    public void ValidateRoster_NoDuplicateIssuesForSameUnit()
    {
        // Arrange
        var units = new[]
        {
            CreateUnit("t1-infantry-1", 1, UnitRole.Infantry, 100, 100),
            CreateUnit("t1-infantry-2", 1, UnitRole.Infantry, 80, 80),
            CreateUnit("t2-infantry", 2, UnitRole.Infantry, 50, 50)  // Below both Tier 1 units
        };

        // Act
        var issues = _validator.ValidateRoster(units).ToList();

        // Assert - should have exactly one issue for the Tier 2 unit
        var t2Issues = issues.Where(i => i.UnitId == "t2-infantry").ToList();
        Assert.Single(t2Issues);
    }

    [Fact]
    public void ValidateRoster_NoTier1UnitsOfRoleDoesNotGenerateIssue()
    {
        // Arrange
        var units = new[]
        {
            CreateUnit("t2-ranged", 2, UnitRole.Ranged, 50, 50),  // No Tier 1 Ranged units to compare
            CreateUnit("t3-ranged", 3, UnitRole.Ranged, 30, 30)
        };

        // Act
        var issues = _validator.ValidateRoster(units);

        // Assert
        Assert.Empty(issues);
    }

    [Fact]
    public void ValidateRoster_ThrowsOnNullRoster()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _validator.ValidateRoster(null!));
    }

    [Fact]
    public void ValidateRoster_EmptyRosterReturnsNoIssues()
    {
        // Act
        var issues = _validator.ValidateRoster(new List<UnitDefinition>());

        // Assert
        Assert.Empty(issues);
    }

    [Fact]
    public void ValidateRoster_IssueIdentifiesAffectedUnitAndComparator()
    {
        // Arrange
        var units = new[]
        {
            CreateUnit("baseline", 1, UnitRole.Infantry, 100, 100),
            CreateUnit("offender", 2, UnitRole.Infantry, 50, 50)
        };

        // Act
        var issues = _validator.ValidateRoster(units).ToList();

        // Assert
        var issue = Assert.Single(issues);
        Assert.Equal("offender", issue.UnitId);
        Assert.Contains("Tier 2", issue.Message); // Should identify the higher tier
        Assert.Contains("Tier 1", issue.Message); // Should reference the comparison tier
    }
}
