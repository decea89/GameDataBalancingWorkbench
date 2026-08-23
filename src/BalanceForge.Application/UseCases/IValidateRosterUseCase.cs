namespace BalanceForge.Application.UseCases;

using BalanceForge.Application.Results;
using BalanceForge.Domain;

/// <summary>
/// Validates a roster and returns all issues.
/// Runs both single-unit and cross-unit validation rules.
/// </summary>
public interface IValidateRosterUseCase
{
    /// <summary>
    /// Validates a roster and returns the aggregated result.
    /// </summary>
    Task<RosterValidationResult> ExecuteAsync(IEnumerable<UnitDefinition> units, CancellationToken cancellationToken = default);
}
