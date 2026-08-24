namespace BalanceForge.Application.UndoRedo;

/// <summary>
/// Immutable record representing a single unit field edit that can be undone/redone.
/// Captures the unit ID, property name, old value, and new value.
/// </summary>
public record UnitEditCommand(
    string UnitId,
    string PropertyName,
    object? OldValue,
    object? NewValue)
{
    /// <summary>
    /// Gets a human-readable description of the edit (e.g., "Health: 100 → 150").
    /// </summary>
    public string Description => $"{PropertyName}: {OldValue} → {NewValue}";
}
