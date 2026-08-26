namespace BalanceForge.Application.Analysis;

using BalanceForge.Domain;

/// <summary>
/// Produces deterministic, explainable diagnostics by comparing units with tier medians.
/// Diagnostics are signals for review, not automatic balance decisions.
/// </summary>
public sealed class BalanceOutlierAnalysisService
{
    public const double WatchThreshold = 0.35;
    public const double OutlierThreshold = 0.75;

    private readonly BalanceMetricsCalculator _metricsCalculator;

    public BalanceOutlierAnalysisService(BalanceMetricsCalculator metricsCalculator)
    {
        _metricsCalculator = metricsCalculator ?? throw new ArgumentNullException(nameof(metricsCalculator));
    }

    public IReadOnlyDictionary<string, UnitOutlierAnalysis> Analyze(
        IEnumerable<UnitDefinition> units)
    {
        ArgumentNullException.ThrowIfNull(units);

        var unitList = units.ToList();
        var results = new Dictionary<string, UnitOutlierAnalysis>(StringComparer.OrdinalIgnoreCase);

        foreach (var tierGroup in unitList.GroupBy(unit => unit.Tier))
        {
            var peers = tierGroup.ToList();
            if (peers.Count < 2)
            {
                foreach (var unit in peers)
                {
                    results[unit.Id] = new UnitOutlierAnalysis(
                        unit.Id,
                        unit.Tier,
                        peers.Count,
                        OutlierClassification.InsufficientPeers,
                        null,
                        Array.Empty<MetricDeviation>());
                }

                continue;
            }

            var calculated = peers
                .Select(unit => (Unit: unit, Metrics: _metricsCalculator.Calculate(unit)))
                .ToList();
            var costMedian = Median(calculated.Select(item => (double)item.Metrics.TotalCost));
            var dpsMedian = Median(calculated.Select(item => item.Metrics.DamagePerSecond));
            var healthMedian = Median(calculated.Select(item => item.Metrics.EffectiveHealth));

            foreach (var item in calculated)
            {
                var deviations = new[]
                {
                    CreateDeviation("Cost", item.Metrics.TotalCost, costMedian),
                    CreateDeviation("DPS", item.Metrics.DamagePerSecond, dpsMedian),
                    CreateDeviation("Effective Health", item.Metrics.EffectiveHealth, healthMedian)
                };
                var strongest = deviations.MaxBy(deviation => Math.Abs(deviation.Percentage))!;
                var absoluteDeviation = Math.Abs(strongest.Percentage);
                var classification = absoluteDeviation >= OutlierThreshold
                    ? OutlierClassification.Outlier
                    : absoluteDeviation >= WatchThreshold
                        ? OutlierClassification.Watch
                        : OutlierClassification.Balanced;

                results[item.Unit.Id] = new UnitOutlierAnalysis(
                    item.Unit.Id,
                    item.Unit.Tier,
                    peers.Count,
                    classification,
                    strongest,
                    deviations);
            }
        }

        return results;
    }

    private static MetricDeviation CreateDeviation(string name, double value, double benchmark)
    {
        var percentage = Math.Abs(benchmark) < 0.0001
            ? Math.Abs(value) < 0.0001 ? 0 : 1
            : (value - benchmark) / Math.Abs(benchmark);
        return new MetricDeviation(name, value, benchmark, percentage);
    }

    private static double Median(IEnumerable<double> values)
    {
        var ordered = values.OrderBy(value => value).ToArray();
        if (ordered.Length == 0)
        {
            return 0;
        }

        var middle = ordered.Length / 2;
        return ordered.Length % 2 == 0
            ? (ordered[middle - 1] + ordered[middle]) / 2
            : ordered[middle];
    }
}
