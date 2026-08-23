namespace BalanceForge.Application.UseCases;

using BalanceForge.Application.Results;
using BalanceForge.Domain;

/// <summary>
/// Validates a roster by running both single-unit and cross-unit rules.
/// </summary>
public class ValidateRosterUseCase : IValidateRosterUseCase
{
    private readonly UnitValidationService _validationService;

    public ValidateRosterUseCase(UnitValidationService validationService)
    {
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
    }

    /// <inheritdoc />
    public Task<RosterValidationResult> ExecuteAsync(IEnumerable<UnitDefinition> units, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(units);

        var unitsList = units.ToList();

        // Run single-unit validation for each unit
        var singleUnitIssues = unitsList
            .SelectMany(unit => _validationService.Validate(unit))
            .ToList();

        // Run cross-unit validation
        var rosterIssues = _validationService.ValidateRoster(unitsList).ToList();

        // Combine all issues
        var allIssues = singleUnitIssues.Concat(rosterIssues).Distinct().ToList();

        var result = RosterValidationResult.From(allIssues);
        return Task.FromResult(result);
    }
}
