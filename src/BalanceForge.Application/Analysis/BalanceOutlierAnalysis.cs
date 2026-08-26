namespace BalanceForge.Application.Analysis;

using BalanceForge.Domain;

public enum OutlierClassification
{
    InsufficientPeers,
    Balanced,
    Watch,
    Outlier
}

public sealed record MetricDeviation(
    string MetricName,
    double Value,
    double Benchmark,
    double Percentage);

public sealed record UnitOutlierAnalysis(
    string UnitId,
    int Tier,
    int PeerCount,
    OutlierClassification Classification,
    MetricDeviation? StrongestDeviation,
    IReadOnlyList<MetricDeviation> Deviations)
{
    public bool IsFlagged =>
        Classification is OutlierClassification.Watch or OutlierClassification.Outlier;
}
