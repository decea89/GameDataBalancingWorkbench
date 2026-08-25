namespace BalanceForge.Desktop.ViewModels;

using System.Collections.ObjectModel;
using BalanceForge.Application;
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

    [ObservableProperty]
    private ObservableCollection<ChartDataPoint> chartData = new();

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

        // Populate chart data
        ChartData.Clear();

        foreach (var unit in unitList)
        {
            if (unit.UnitDefinition == null)
                continue;

            var point = new ChartDataPoint
            {
                UnitName = unit.DisplayName,
                TotalCost = (decimal)unit.TotalCost,
                DPS = (decimal)unit.DPS,
                EffectiveHealth = (decimal)unit.EffectiveHealth
            };

            ChartData.Add(point);
        }

        HasData = ChartData.Count > 0;
    }

    private void ClearChart()
    {
        ChartData.Clear();
        HasData = false;
    }
}

/// <summary>
/// Simple data point for chart display.
/// Used by BalanceChartView to show metrics for each unit.
/// </summary>
public class ChartDataPoint
{
    public string UnitName { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public decimal DPS { get; set; }
    public decimal EffectiveHealth { get; set; }
}
