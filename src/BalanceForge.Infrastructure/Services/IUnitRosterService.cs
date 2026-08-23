namespace BalanceForge.Infrastructure.Services;

using BalanceForge.Domain;

/// <summary>
/// Service contract for loading and saving unit roster data.
/// Orchestrates JSON serialization, deserialization, and domain mapping.
/// </summary>
public interface IUnitRosterService
{
    /// <summary>
    /// Loads units from a JSON file asynchronously.
    /// Returns a list of domain UnitDefinition objects.
    /// Throws on file not found, JSON parse errors, or invalid role mappings.
    /// </summary>
    Task<IReadOnlyList<UnitDefinition>> LoadAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves units to a JSON file asynchronously.
    /// Overwrites existing file if present. Creates parent directories if needed.
    /// Throws on validation errors, serialization failures, or I/O errors.
    /// </summary>
    Task SaveAsync(string filePath, IEnumerable<UnitDefinition> units, CancellationToken cancellationToken = default);
}
