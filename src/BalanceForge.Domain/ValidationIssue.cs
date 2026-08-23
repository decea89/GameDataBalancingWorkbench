namespace BalanceForge.Domain;

/// <summary>
/// Represents a single validation issue found during unit validation.
/// Immutable result identifying the affected unit, severity, rule, and guidance.
/// </summary>
public record ValidationIssue(
    string RuleId,
    ValidationSeverity Severity,
    string UnitId,
    string Message,
    string SuggestedAction
);
