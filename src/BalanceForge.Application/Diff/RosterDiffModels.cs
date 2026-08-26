namespace BalanceForge.Application.Diff;

public enum UnitChangeKind
{
    Added,
    Removed,
    Modified,
    Unchanged
}

public sealed record FieldDelta(
    string FieldName,
    string BaselineValue,
    string CurrentValue,
    double? NumericDelta = null,
    double? PercentageDelta = null);

public sealed record UnitDiff(
    string UnitId,
    string DisplayName,
    UnitChangeKind ChangeKind,
    IReadOnlyList<FieldDelta> FieldDeltas);

public sealed record RosterDiffResult(
    IReadOnlyList<UnitDiff> Units,
    int AddedCount,
    int RemovedCount,
    int ModifiedCount,
    int UnchangedCount)
{
    public int ChangedCount => AddedCount + RemovedCount + ModifiedCount;
}

