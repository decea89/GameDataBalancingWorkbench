namespace BalanceForge.Desktop.ViewModels;

using System.Collections.ObjectModel;
using BalanceForge.Application;
using BalanceForge.Domain;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveCharts;
using LiveCharts.SkiaSharp;

/// <summary>
/// View model for the balance analysis chart.
/// Prepares chart data from displayed units, showing Total Cost, DPS, and Effective Health.
/// No WPF dependencies; reuses existing BalanceMetricsCalculator.
/// </summary>
public partial class BalanceChartViewModel : ObservableObject
{
    private readonly BalanceMetricsCalculator _metricsCalculator;

    [ObservableProperty]
    private ObservableCollection<ISeries> seriesCollection = new();

    [ObservableProperty]
    private List<string> labels = new();

    [ObservableProperty]
    private string chartTitle = "Unit Balance Comparison";

    [ObservableProperty]
    private bool hasData = false;

    public BalanceChartViewModel(BalanceMetricsCalculator metricsCalculator)
    {
        _metricsCalculator = metricsCalculator ?? throw new ArgumentNullException(nameof(metricsCalculator));
    }

    /// <summary>
    /// Updates chart data from the given list of units.
    /// </summary>
    public void UpdateChartData(IEnumerable<RosterUnitViewModel> displayedUnits)
    {
        if (displayedUnits == null)
        {
            ClearChart();
            return;
        }

        var unitList = displayedUnits.ToList();

        if (unitList.Count == 0)
        {
            ClearChart();
            return;
        }

        // Prepare labels (unit display names)
        Labels = unitList.Select(u => u.DisplayName).ToList();

        // Calculate metrics for each unit
        var totalCosts = new List<double>();
        var dpsList = new List<double>();
        var effectiveHealths = new List<double>();

        foreach (var unit in unitList)
        {
            var metrics = _metricsCalculator.Calculate(unit.UnitDefinition);
            totalCosts.Add(metrics.TotalCost);
            dpsList.Add(metrics.DamagePerSecond);
            effectiveHealths.Add(metrics.EffectiveHealth);
        }

        // Create series for the chart
        var series = new ObservableCollection<ISeries>
        {
            new ColumnSeries<double>
            {
                Title = "Total Cost",
                Values = totalCosts.AsLiveChartsBindingList(),
                Fill = new SolidColorPaint(SKColors.RoyalBlue),
            },
            new ColumnSeries<double>
            {
                Title = "DPS",
                Values = dpsList.AsLiveChartsBindingList(),
                Fill = new SolidColorPaint(SKColors.OrangeRed),
            },
            new ColumnSeries<double>
            {
                Title = "Effective Health",
                Values = effectiveHealths.AsLiveChartsBindingList(),
                Fill = new SolidColorPaint(SKColors.ForestGreen),
            },
        };

        SeriesCollection = series;
        HasData = true;
    }

    /// <summary>
    /// Clears chart data and shows empty state.
    /// </summary>
    private void ClearChart()
    {
        SeriesCollection = new ObservableCollection<ISeries>();
        Labels = new List<string>();
        HasData = false;
    }
}
