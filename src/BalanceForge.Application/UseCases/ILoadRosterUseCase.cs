namespace BalanceForge.Application.UseCases;

using BalanceForge.Application.Results;
using BalanceForge.Domain;

/// <summary>
/// Loads a roster from a JSON file and optionally validates it.
/// Returns the loaded units with any validation issues.
/// </summary>
public interface ILoadRosterUseCase
{
    /// <summary>
    /// Loads units from a file, optionally runs validation, and returns the result.
    /// </summary>
    Task<RosterLoadResult> ExecuteAsync(string filePath, bool validateOnLoad = true, CancellationToken cancellationToken = default);
}
