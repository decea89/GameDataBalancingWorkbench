namespace BalanceForge.Application.Results;

using BalanceForge.Domain;

/// <summary>
/// Result of a roster load operation.
/// Includes the loaded units and any validation issues encountered.
/// </summary>
public record RosterLoadResult(
    IReadOnlyList<UnitDefinition> Units,
    IReadOnlyList<ValidationIssue> ValidationIssues,
    bool HasErrors)
{
    /// <summary>
    /// Creates a successful load result with units.
    /// </summary>
    public static RosterLoadResult Success(IReadOnlyList<UnitDefinition> units, IReadOnlyList<ValidationIssue> validationIssues)
    {
        var hasErrors = validationIssues.Any(i => i.Severity == ValidationSeverity.Error);
        return new RosterLoadResult(units, validationIssues, hasErrors);
    }

    /// <summary>
    /// Creates a failure result (e.g., file not found, parse error).
    /// </summary>
    public static RosterLoadResult Failure(string message)
    {
        throw new InvalidOperationException($"Load failed: {message}");
    }
}
