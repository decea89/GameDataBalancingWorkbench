namespace BalanceForge.Desktop.Views;

using System.Windows;
using System.Windows.Controls;
using BalanceForge.Desktop.ViewModels;

/// <summary>
/// Interaction logic for ComparisonView.xaml
/// </summary>
public partial class ComparisonView : UserControl
{
    public ComparisonView()
    {
        InitializeComponent();
        DataContextChanged += ComparisonView_DataContextChanged;
    }

    private void ComparisonView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (DataContext is ComparisonViewModel vm)
        {
            UpdateStatsList(vm);
            UpdateMetricsList(vm);
        }
    }

    private void UpdateStatsList(ComparisonViewModel vm)
    {
        if (StatsItemsControl == null)
            return;

        var statNames = new[]
        {
            "Health", "Damage", "Atk/Sec", "Armor", "Range",
            "Wood Cost", "Gold Cost", "Pop Cost", "Prod Time"
        };

        var stats = new List<StatComparison>();
        foreach (var name in statNames)
        {
            var stat = vm.GetStatComparison(name);
            if (stat != null)
                stats.Add(stat);
        }

        StatsItemsControl.ItemsSource = stats;
    }

    private void UpdateMetricsList(ComparisonViewModel vm)
    {
        if (MetricsItemsControl == null)
            return;

        var metricNames = new[] { "Total Cost", "DPS", "DPS/Cost", "Effective Health" };
        var metrics = new List<MetricComparison>();

        foreach (var name in metricNames)
        {
            var metric = vm.GetMetricComparison(name);
            if (metric != null)
                metrics.Add(metric);
        }

        MetricsItemsControl.ItemsSource = metrics;
    }
}
