namespace BalanceForge.Application.Tests;

using BalanceForge.Application;
using BalanceForge.Application.Analysis;
using BalanceForge.Domain;

public class BalanceOutlierAnalysisServiceTests
{
    [Fact]
    public void Analyze_UsesTierMedianAndFlagsStrongDeviation()
    {
        var service = new BalanceOutlierAnalysisService(new BalanceMetricsCalculator());
        var units = new[]
        {
            CreateUnit("a", 1, damage: 10),
            CreateUnit("b", 1, damage: 10),
            CreateUnit("c", 1, damage: 40)
        };

        var result = service.Analyze(units);

        Assert.Equal(OutlierClassification.Outlier, result["c"].Classification);
        Assert.Equal("DPS", result["c"].StrongestDeviation?.MetricName);
        Assert.Equal(3, result["c"].PeerCount);
    }

    [Fact]
    public void Analyze_WithSinglePeer_ReturnsInsufficientPeers()
    {
        var service = new BalanceOutlierAnalysisService(new BalanceMetricsCalculator());

        var result = service.Analyze(new[] { CreateUnit("solo", 4, damage: 10) });

        Assert.Equal(OutlierClassification.InsufficientPeers, result["solo"].Classification);
        Assert.Null(result["solo"].StrongestDeviation);
        Assert.False(result["solo"].IsFlagged);
    }

    [Theory]
    [InlineData(13, OutlierClassification.Balanced)]
    [InlineData(14, OutlierClassification.Watch)]
    [InlineData(20, OutlierClassification.Outlier)]
    public void Analyze_AppliesClassificationThresholds(
        double damage,
        OutlierClassification expected)
    {
        var service = new BalanceOutlierAnalysisService(new BalanceMetricsCalculator());
        var units = new[]
        {
            CreateUnit("baseline-a", 1, damage: 10),
            CreateUnit("baseline-b", 1, damage: 10),
            CreateUnit("candidate", 1, damage)
        };

        var result = service.Analyze(units);

        Assert.Equal(expected, result["candidate"].Classification);
    }

    private static UnitDefinition CreateUnit(string id, int tier, double damage)
    {
        return new UnitDefinition
        {
            Id = id,
            DisplayName = id,
            Role = UnitRole.Infantry,
            Tier = tier,
            Health = 100,
            Damage = damage,
            AttacksPerSecond = 1,
            Armor = 0,
            Range = 1,
            WoodCost = 50,
            GoldCost = 50,
            PopulationCost = 1,
            ProductionTimeSeconds = 10
        };
    }
}
