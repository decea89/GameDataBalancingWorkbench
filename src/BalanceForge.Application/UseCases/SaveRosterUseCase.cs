namespace BalanceForge.Application.UseCases;

using BalanceForge.Application.Results;
using BalanceForge.Application.Services;
using BalanceForge.Domain;

/// <summary>
/// Saves a roster to a JSON file after validating it.
/// Prevents saving rosters with error-level issues.
/// </summary>
public class SaveRosterUseCase : ISaveRosterUseCase
{
    private readonly IUnitRosterService _rosterService;
    private readonly IValidateRosterUseCase _validateUseCase;

    public SaveRosterUseCase(IUnitRosterService rosterService, IValidateRosterUseCase validateUseCase)
    {
        _rosterService = rosterService ?? throw new ArgumentNullException(nameof(rosterService));
        _validateUseCase = validateUseCase ?? throw new ArgumentNullException(nameof(validateUseCase));
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(string filePath, IEnumerable<UnitDefinition> units, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        ArgumentNullException.ThrowIfNull(units);

        var unitsList = units.ToList();

        // Validate before saving
        var validationResult = await _validateUseCase.ExecuteAsync(unitsList, cancellationToken);

        // Prevent saving if there are error-level issues
        if (validationResult.HasErrors)
        {
            throw new InvalidOperationException(
                $"Cannot save roster with {validationResult.ErrorCount} validation error(s). " +
                "Fix all errors before saving.");
        }

        // Save to file (warnings are allowed)
        await _rosterService.SaveAsync(filePath, unitsList, cancellationToken);
    }
}
