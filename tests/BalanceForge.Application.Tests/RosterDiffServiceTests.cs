namespace BalanceForge.Application.Tests;

using BalanceForge.Application.Diff;
using BalanceForge.Domain;

public class RosterDiffServiceTests
{
    private readonly RosterDiffService _service = new();

    [Fact]
    public void Compare_ClassifiesAddedRemovedModifiedAndUnchangedUnits()
    {
        var baseline = new[]
        {
            CreateUnit("unchanged"),
            CreateUnit("modified", health: 100),
            CreateUnit("removed")
        };
        var current = new[]
        {
            CreateUnit("unchanged"),
            CreateUnit("modified", health: 125),
            CreateUnit("added")
        };

        var result = _service.Compare(baseline, current);

        Assert.Equal(1, result.AddedCount);
        Assert.Equal(1, result.RemovedCount);
        Assert.Equal(1, result.ModifiedCount);
        Assert.Equal(1, result.UnchangedCount);
        Assert.Equal(UnitChangeKind.Added, result.Units.Single(unit => unit.UnitId == "added").ChangeKind);
        Assert.Equal(UnitChangeKind.Removed, result.Units.Single(unit => unit.UnitId == "removed").ChangeKind);
    }

    [Fact]
    public void Compare_ModifiedNumericFieldIncludesAbsoluteAndPercentageDelta()
    {
        var baseline = new[] { CreateUnit("knight", health: 100) };
        var current = new[] { CreateUnit("knight", health: 125) };

        var result = _service.Compare(baseline, current);

        var health = Assert.Single(result.Units[0].FieldDeltas);
        Assert.Equal("Health", health.FieldName);
        Assert.Equal(25d, health.NumericDelta);
        Assert.Equal(25d, health.PercentageDelta);
    }

    [Fact]
    public void Compare_ZeroBaselineDoesNotProduceInvalidPercentage()
    {
        var baseline = new[] { CreateUnit("cleric", damage: 0) };
        var current = new[] { CreateUnit("cleric", damage: 5) };

        var result = _service.Compare(baseline, current);

        var damage = Assert.Single(result.Units[0].FieldDeltas);
        Assert.Equal(5d, damage.NumericDelta);
        Assert.Null(damage.PercentageDelta);
    }

    [Fact]
    public void Compare_UsesCaseInsensitiveUnitIds()
    {
        var baseline = new[] { CreateUnit("Knight") };
        var current = new[] { CreateUnit("knight") };
        current[0].DisplayName = baseline[0].DisplayName;

        var result = _service.Compare(baseline, current);

        Assert.Equal(0, result.ChangedCount);
        Assert.Equal(1, result.UnchangedCount);
    }

    private static UnitDefinition CreateUnit(
        string id,
        double health = 100,
        double damage = 10)
    {
        return new UnitDefinition
        {
            Id = id,
            DisplayName = id,
            Role = UnitRole.Infantry,
            Tier = 1,
            Health = health,
            Damage = damage,
            AttacksPerSecond = 1,
            Armor = 1,
            Range = 1,
            WoodCost = 50,
            GoldCost = 25,
            PopulationCost = 1,
            ProductionTimeSeconds = 10
        };
    }
}
