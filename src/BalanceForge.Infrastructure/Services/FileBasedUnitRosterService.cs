namespace BalanceForge.Infrastructure.Services;

using System.Text.Json;
using BalanceForge.Application.Services;
using BalanceForge.Domain;
using BalanceForge.Infrastructure.Mapping;
using BalanceForge.Infrastructure.Models;

/// <summary>
/// File-based implementation of IUnitRosterService.
/// Loads/saves units.json with System.Text.Json serialization.
/// </summary>
public class FileBasedUnitRosterService : IUnitRosterService
{
    private readonly IFileAccessor _fileAccessor;

    /// <summary>
    /// Default JSON serializer options configured for camelCase property names.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
    };

    public FileBasedUnitRosterService(IFileAccessor fileAccessor)
    {
        _fileAccessor = fileAccessor ?? throw new ArgumentNullException(nameof(fileAccessor));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UnitDefinition>> LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        try
        {
            // Read file
            var json = await _fileAccessor.ReadAllTextAsync(filePath, cancellationToken);

            // Deserialize to JSON model
            var roster = JsonSerializer.Deserialize<RosterJsonModel>(json, JsonOptions)
                ?? throw new InvalidOperationException("Deserialized roster is null.");

            if (roster.Units is null)
            {
                throw new InvalidOperationException("Roster units list is null.");
            }

            // Map to domain models
            var units = UnitMapper.FromJsonRoster(roster.Units).ToList();
            return units.AsReadOnly();
        }
        catch (FileNotFoundException ex)
        {
            throw new FileNotFoundException($"Units file not found at '{filePath}'.", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse JSON from '{filePath}'.", ex);
        }
        catch (InvalidOperationException ex) when (!ex.Message.Contains("not found"))
        {
            // Re-throw InvalidOperationException from mapping (e.g., unknown role)
            throw;
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(string filePath, IEnumerable<UnitDefinition> units, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        ArgumentNullException.ThrowIfNull(units);

        try
        {
            // Map domain units to JSON models
            var jsonUnits = UnitMapper.ToJsonRoster(units).ToList();

            // Create roster model
            var roster = new RosterJsonModel { Units = jsonUnits };

            // Serialize to JSON
            var json = JsonSerializer.Serialize(roster, JsonOptions);

            // Write to file
            await _fileAccessor.WriteAllTextAsync(filePath, json, cancellationToken);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to serialize roster to JSON.", ex);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"Failed to write roster to '{filePath}'.", ex);
        }
    }
}
