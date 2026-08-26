namespace BalanceForge.Application.Diff;

using System.Globalization;
using BalanceForge.Domain;

/// <summary>
/// Produces a deterministic, field-level comparison between two unit rosters.
/// </summary>
public sealed class RosterDiffService
{
    private const double NumericTolerance = 0.0001;

    public RosterDiffResult Compare(
        IEnumerable<UnitDefinition> baselineUnits,
        IEnumerable<UnitDefinition> currentUnits)
    {
        ArgumentNullException.ThrowIfNull(baselineUnits);
        ArgumentNullException.ThrowIfNull(currentUnits);

        var baseline = baselineUnits.ToDictionary(unit => unit.Id, StringComparer.OrdinalIgnoreCase);
        var current = currentUnits.ToDictionary(unit => unit.Id, StringComparer.OrdinalIgnoreCase);
        var ids = baseline.Keys
            .Union(current.Keys, StringComparer.OrdinalIgnoreCase)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase);
        var differences = new List<UnitDiff>();

        foreach (var id in ids)
        {
            var hasBaseline = baseline.TryGetValue(id, out var baselineUnit);
            var hasCurrent = current.TryGetValue(id, out var currentUnit);

            if (!hasBaseline)
            {
                differences.Add(new UnitDiff(
                    id,
                    currentUnit!.DisplayName,
                    UnitChangeKind.Added,
                    Array.Empty<FieldDelta>()));
                continue;
            }

            if (!hasCurrent)
            {
                differences.Add(new UnitDiff(
                    id,
                    baselineUnit!.DisplayName,
                    UnitChangeKind.Removed,
                    Array.Empty<FieldDelta>()));
                continue;
            }

            var fieldDeltas = CompareFields(baselineUnit!, currentUnit!);
            differences.Add(new UnitDiff(
                id,
                currentUnit!.DisplayName,
                fieldDeltas.Count == 0 ? UnitChangeKind.Unchanged : UnitChangeKind.Modified,
                fieldDeltas));
        }

        return new RosterDiffResult(
            differences,
            differences.Count(unit => unit.ChangeKind == UnitChangeKind.Added),
            differences.Count(unit => unit.ChangeKind == UnitChangeKind.Removed),
            differences.Count(unit => unit.ChangeKind == UnitChangeKind.Modified),
            differences.Count(unit => unit.ChangeKind == UnitChangeKind.Unchanged));
    }

    private static IReadOnlyList<FieldDelta> CompareFields(
        UnitDefinition baseline,
        UnitDefinition current)
    {
        var deltas = new List<FieldDelta>();

        AddTextDelta(deltas, "Display Name", baseline.DisplayName, current.DisplayName);
        AddTextDelta(deltas, "Image Path", baseline.ImagePath ?? string.Empty, current.ImagePath ?? string.Empty);
        AddTextDelta(deltas, "Role", baseline.Role.ToString(), current.Role.ToString());
        AddNumericDelta(deltas, "Tier", baseline.Tier, current.Tier);
        AddNumericDelta(deltas, "Health", baseline.Health, current.Health);
        AddNumericDelta(deltas, "Damage", baseline.Damage, current.Damage);
        AddNumericDelta(deltas, "Attacks / Second", baseline.AttacksPerSecond, current.AttacksPerSecond);
        AddNumericDelta(deltas, "Armor", baseline.Armor, current.Armor);
        AddNumericDelta(deltas, "Range", baseline.Range, current.Range);
        AddNumericDelta(deltas, "Wood Cost", baseline.WoodCost, current.WoodCost);
        AddNumericDelta(deltas, "Gold Cost", baseline.GoldCost, current.GoldCost);
        AddNumericDelta(deltas, "Population Cost", baseline.PopulationCost, current.PopulationCost);
        AddNumericDelta(deltas, "Production Time", baseline.ProductionTimeSeconds, current.ProductionTimeSeconds);
        AddTextDelta(
            deltas,
            "Allow Cost Tier Inversion",
            baseline.AllowCostTierInversion.ToString(),
            current.AllowCostTierInversion.ToString());

        return deltas;
    }

    private static void AddTextDelta(
        ICollection<FieldDelta> deltas,
        string fieldName,
        string baseline,
        string current)
    {
        if (!string.Equals(baseline, current, StringComparison.Ordinal))
        {
            deltas.Add(new FieldDelta(fieldName, baseline, current));
        }
    }

    private static void AddNumericDelta(
        ICollection<FieldDelta> deltas,
        string fieldName,
        double baseline,
        double current)
    {
        if (Math.Abs(current - baseline) <= NumericTolerance)
        {
            return;
        }

        var delta = current - baseline;
        double? percentage = Math.Abs(baseline) <= NumericTolerance
            ? null
            : delta / Math.Abs(baseline) * 100d;
        deltas.Add(new FieldDelta(
            fieldName,
            FormatNumber(baseline),
            FormatNumber(current),
            delta,
            percentage));
    }

    private static string FormatNumber(double value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture);
}
