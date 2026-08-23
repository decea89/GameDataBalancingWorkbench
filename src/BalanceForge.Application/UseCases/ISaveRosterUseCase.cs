namespace BalanceForge.Application.UseCases;

using BalanceForge.Application.Results;
using BalanceForge.Domain;

/// <summary>
/// Saves a roster to a JSON file after validation.
/// Prevents saving rosters with error-level validation issues.
/// </summary>
public interface ISaveRosterUseCase
{
    /// <summary>
    /// Validates the roster and saves it if validation passes.
    /// Throws if there are error-level issues.
    /// </summary>
    Task ExecuteAsync(string filePath, IEnumerable<UnitDefinition> units, CancellationToken cancellationToken = default);
}
