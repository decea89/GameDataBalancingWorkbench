namespace BalanceForge.Application.Results;

using BalanceForge.Domain;

/// <summary>
/// Result of roster-level validation.
/// Aggregates all validation issues (single-unit and cross-unit rules).
/// </summary>
public record RosterValidationResult(
    IReadOnlyList<ValidationIssue> AllIssues,
    int ErrorCount,
    int WarningCount,
    int InfoCount)
{
    /// <summary>
    /// True if there are any error-level issues.
    /// </summary>
    public bool HasErrors => ErrorCount > 0;

    /// <summary>
    /// Creates a validation result from a collection of issues.
    /// </summary>
    public static RosterValidationResult From(IEnumerable<ValidationIssue> issues)
    {
        var issueList = issues.ToList();
        var errorCount = issueList.Count(i => i.Severity == ValidationSeverity.Error);
        var warningCount = issueList.Count(i => i.Severity == ValidationSeverity.Warning);
        var infoCount = issueList.Count(i => i.Severity == ValidationSeverity.Info);

        return new RosterValidationResult(issueList.AsReadOnly(), errorCount, warningCount, infoCount);
    }
}
