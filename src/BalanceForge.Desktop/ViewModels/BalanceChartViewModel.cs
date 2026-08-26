namespace BalanceForge.Desktop.ViewModels;

using System.Collections.ObjectModel;
using BalanceForge.Application;
using BalanceForge.Application.Analysis;
using BalanceForge.Domain;
using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>
/// View model for the balance analysis chart.
/// Prepares chart data from displayed units, showing Total Cost, DPS, and Effective Health.
/// No WPF dependencies; reuses existing BalanceMetricsCalculator.
/// </summary>
public partial class BalanceChartViewModel : ObservableObject
{
    private readonly BalanceMetricsCalculator _metricsCalculator;
    private readonly BalanceOutlierAnalysisService _outlierAnalysisService;

    [ObservableProperty]
    private ObservableCollection<ChartDataPoint> chartData = new();

    [ObservableProperty]
    private string chartTitle = "Unit Balance Comparison";

    [ObservableProperty]
    private bool hasData = false;

    [ObservableProperty]
    private int outlierCount;

    public BalanceChartViewModel(BalanceMetricsCalculator metricsCalculator)
    {
        _metricsCalculator = metricsCalculator ?? throw new ArgumentNullException(nameof(metricsCalculator));
        _outlierAnalysisService = new BalanceOutlierAnalysisService(_metricsCalculator);
    }

    /// <summary>
    /// Updates chart data from the given list of units.
    /// </summary>
    public void UpdateChartData(
        IEnumerable<RosterUnitViewModel> displayedUnits,
        IEnumerable<UnitDefinition>? benchmarkUnits = null)
    {
        if (displayedUnits == null)
        {
            ClearChart();
            return;
        }

        var unitList = displayedUnits.ToList();
        var diagnostics = _outlierAnalysisService.Analyze(
            benchmarkUnits ?? unitList.Select(unit => unit.UnitDefinition));

        if (unitList.Count == 0)
        {
            ClearChart();
            return;
        }

        // Populate chart data
        ChartData.Clear();

        foreach (var unit in unitList)
        {
            if (unit.UnitDefinition == null)
                continue;

            var point = new ChartDataPoint
            {
                UnitName = unit.DisplayName,
                UnitInitial = unit.UnitInitial,
                ImageSourcePath = unit.ImageSourcePath,
                HasImage = unit.HasImage,
                TotalCost = (decimal)unit.TotalCost,
                DPS = (decimal)unit.DPS,
                EffectiveHealth = (decimal)unit.EffectiveHealth
            };

            if (diagnostics.TryGetValue(unit.Id, out var diagnostic))
            {
                point.DiagnosticStatus = diagnostic.Classification switch
                {
                    OutlierClassification.Outlier => "Outlier",
                    OutlierClassification.Watch => "Watch",
                    OutlierClassification.Balanced => "Balanced",
                    _ => "No benchmark"
                };
                point.DiagnosticColor = diagnostic.Classification switch
                {
                    OutlierClassification.Outlier => "#A13544",
                    OutlierClassification.Watch => "#964219",
                    OutlierClassification.Balanced => "#437A22",
                    _ => "#6B7280"
                };
                point.IsFlagged = diagnostic.IsFlagged;
                point.DiagnosticInsight = diagnostic.StrongestDeviation == null
                    ? "Not enough units in this tier"
                    : $"{diagnostic.StrongestDeviation.MetricName} " +
                      $"{diagnostic.StrongestDeviation.Percentage:+0%;-0%;0%} vs Tier {diagnostic.Tier} median";
            }

            ChartData.Add(point);
        }

        HasData = ChartData.Count > 0;
        OutlierCount = ChartData.Count(point => point.IsFlagged);
    }

    private void ClearChart()
    {
        ChartData.Clear();
        HasData = false;
        OutlierCount = 0;
    }
}

/// <summary>
/// Simple data point for chart display.
/// Used by BalanceChartView to show metrics for each unit.
/// </summary>
public class ChartDataPoint
{
    public string UnitName { get; set; } = string.Empty;
    public string UnitInitial { get; set; } = "?";
    public string? ImageSourcePath { get; set; }
    public bool HasImage { get; set; }
    public string DiagnosticStatus { get; set; } = "No benchmark";
    public string DiagnosticColor { get; set; } = "#6B7280";
    public string DiagnosticInsight { get; set; } = string.Empty;
    public bool IsFlagged { get; set; }
    public decimal TotalCost { get; set; }
    public decimal DPS { get; set; }
    public decimal EffectiveHealth { get; set; }
}
