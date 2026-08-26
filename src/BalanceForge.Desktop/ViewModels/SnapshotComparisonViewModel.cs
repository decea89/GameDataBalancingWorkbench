namespace BalanceForge.Desktop.ViewModels;

using System.Collections.ObjectModel;
using System.Globalization;
using BalanceForge.Application.Diff;
using BalanceForge.Domain;
using CommunityToolkit.Mvvm.ComponentModel;

public partial class SnapshotComparisonViewModel : ObservableObject
{
    private readonly RosterDiffService _diffService = new();
    private IReadOnlyList<UnitDefinition> _baselineUnits = Array.Empty<UnitDefinition>();

    [ObservableProperty]
    private string baselineFileName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<UnitDiffRowViewModel> differences = new();

    [ObservableProperty]
    private int addedCount;

    [ObservableProperty]
    private int removedCount;

    [ObservableProperty]
    private int modifiedCount;

    [ObservableProperty]
    private int unchangedCount;

    public void SetComparison(
        string baselineFileName,
        IReadOnlyList<UnitDefinition> baselineUnits,
        IEnumerable<UnitDefinition> currentUnits)
    {
        BaselineFileName = baselineFileName;
        _baselineUnits = baselineUnits;
        Refresh(currentUnits);
    }

    public void Refresh(IEnumerable<UnitDefinition> currentUnits)
    {
        if (_baselineUnits.Count == 0)
        {
            return;
        }

        var result = _diffService.Compare(_baselineUnits, currentUnits);
        AddedCount = result.AddedCount;
        RemovedCount = result.RemovedCount;
        ModifiedCount = result.ModifiedCount;
        UnchangedCount = result.UnchangedCount;
        Differences = new ObservableCollection<UnitDiffRowViewModel>(
            result.Units
                .Where(unit => unit.ChangeKind != UnitChangeKind.Unchanged)
                .Select(unit => new UnitDiffRowViewModel(unit)));
    }

    public void Clear()
    {
        _baselineUnits = Array.Empty<UnitDefinition>();
        BaselineFileName = string.Empty;
        Differences.Clear();
        AddedCount = 0;
        RemovedCount = 0;
        ModifiedCount = 0;
        UnchangedCount = 0;
    }
}

public sealed class UnitDiffRowViewModel
{
    public UnitDiffRowViewModel(UnitDiff diff)
    {
        UnitId = diff.UnitId;
        DisplayName = diff.DisplayName;
        ChangeKind = diff.ChangeKind.ToString();
        ChangedFieldCount = diff.FieldDeltas.Count;
        Details = diff.FieldDeltas.Select(FormatDelta).ToList();
        Summary = diff.ChangeKind switch
        {
            UnitChangeKind.Added => "Added to current roster",
            UnitChangeKind.Removed => "Removed from current roster",
            _ when Details.Count == 0 => "No field changes",
            _ => string.Join("  |  ", Details.Take(3))
        };
    }

    public string UnitId { get; }

    public string DisplayName { get; }

    public string ChangeKind { get; }

    public int ChangedFieldCount { get; }

    public string Summary { get; }

    public IReadOnlyList<string> Details { get; }

    private static string FormatDelta(FieldDelta delta)
    {
        var numeric = delta.NumericDelta is null
            ? string.Empty
            : $" ({delta.NumericDelta.Value:+0.##;-0.##;0}{FormatPercentage(delta.PercentageDelta)})";
        return $"{delta.FieldName}: {DisplayValue(delta.BaselineValue)} → {DisplayValue(delta.CurrentValue)}{numeric}";
    }

    private static string FormatPercentage(double? percentage) =>
        percentage is null
            ? string.Empty
            : $", {percentage.Value.ToString("+0.#;-0.#;0", CultureInfo.InvariantCulture)}%";

    private static string DisplayValue(string value) =>
        string.IsNullOrEmpty(value) ? "(empty)" : value;
}

