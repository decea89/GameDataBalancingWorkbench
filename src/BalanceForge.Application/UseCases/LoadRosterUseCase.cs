namespace BalanceForge.Application.UseCases;

using BalanceForge.Application.Results;
using BalanceForge.Application.Services;
using BalanceForge.Domain;

/// <summary>
/// Loads a roster from a JSON file and optionally validates it.
/// </summary>
public class LoadRosterUseCase : ILoadRosterUseCase
{
    private readonly IUnitRosterService _rosterService;
    private readonly IValidateRosterUseCase _validateUseCase;

    public LoadRosterUseCase(IUnitRosterService rosterService, IValidateRosterUseCase validateUseCase)
    {
        _rosterService = rosterService ?? throw new ArgumentNullException(nameof(rosterService));
        _validateUseCase = validateUseCase ?? throw new ArgumentNullException(nameof(validateUseCase));
    }

    /// <inheritdoc />
    public async Task<RosterLoadResult> ExecuteAsync(string filePath, bool validateOnLoad = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        // Load units from file (may throw on I/O or parse errors)
        var units = await _rosterService.LoadAsync(filePath, cancellationToken);

        // Optionally validate
        if (validateOnLoad)
        {
            var validationResult = await _validateUseCase.ExecuteAsync(units, cancellationToken);
            return RosterLoadResult.Success(units, validationResult.AllIssues);
        }

        return RosterLoadResult.Success(units, new List<ValidationIssue>().AsReadOnly());
    }
}
