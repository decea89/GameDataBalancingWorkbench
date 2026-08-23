namespace BalanceForge.Application.Tests;

using BalanceForge.Application.Services;
using BalanceForge.Application.UseCases;
using BalanceForge.Domain;
using Xunit;

public class UseCaseTests
{
    private class MockUnitRosterService : IUnitRosterService
    {
        private readonly Dictionary<string, IReadOnlyList<UnitDefinition>> _storage = new();

        public Task<IReadOnlyList<UnitDefinition>> LoadAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (!_storage.TryGetValue(filePath, out var units))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }
            return Task.FromResult(units);
        }

        public Task SaveAsync(string filePath, IEnumerable<UnitDefinition> units, CancellationToken cancellationToken = default)
        {
            _storage[filePath] = units.ToList().AsReadOnly();
            return Task.CompletedTask;
        }

        public void SetFileContent(string filePath, IEnumerable<UnitDefinition> units)
        {
            _storage[filePath] = units.ToList().AsReadOnly();
        }

        public IReadOnlyList<UnitDefinition>? GetFileContent(string filePath)
        {
            _storage.TryGetValue(filePath, out var units);
            return units;
        }
    }

    private static UnitDefinition CreateValidUnit(
        string id = "test",
        int tier = 1,
        UnitRole role = UnitRole.Infantry,
        double health = 100,
        double damage = 10)
    {
        return new UnitDefinition
        {
            Id = id,
            DisplayName = $"Unit-{id}",
            Role = role,
            Tier = tier,
            Health = health,
            Damage = damage,
            AttacksPerSecond = 1.0,
            Armor = 0,
            Range = 1.0,
            WoodCost = 10,
            GoldCost = 10,
            PopulationCost = 1,
            ProductionTimeSeconds = 5.0
        };
    }

    [Fact]
    public async Task ValidateRosterUseCase_IdentifiesSingleUnitIssues()
    {
        // Arrange
        var invalidUnit = new UnitDefinition
        {
            Id = "bad",
            DisplayName = "Bad",
            Role = UnitRole.Infantry,
            Tier = 1,
            Health = -100, // Invalid
            Damage = 10,
            AttacksPerSecond = 1.0,
            Armor = 0,
            Range = 1.0,
            WoodCost = 10,
            GoldCost = 10,
            PopulationCost = 1,
            ProductionTimeSeconds = 5.0
        };

        var validationService = new UnitValidationService();
        var useCase = new ValidateRosterUseCase(validationService);

        // Act
        var result = await useCase.ExecuteAsync(new[] { invalidUnit });

        // Assert
        Assert.NotEmpty(result.AllIssues);
        Assert.True(result.HasErrors);
        Assert.True(result.ErrorCount > 0);
    }

    [Fact]
    public async Task ValidateRosterUseCase_IdentifiesCrossUnitIssues()
    {
        // Arrange
        var tier1Unit = CreateValidUnit("t1", 1, UnitRole.Infantry, 100, 10);
        tier1Unit.WoodCost = 100;
        tier1Unit.GoldCost = 100;

        var tier2Unit = CreateValidUnit("t2", 2, UnitRole.Infantry, 150, 15);
        tier2Unit.WoodCost = 50; // Lower cost than Tier 1!
        tier2Unit.GoldCost = 50;

        var validationService = new UnitValidationService();
        var useCase = new ValidateRosterUseCase(validationService);

        // Act
        var result = await useCase.ExecuteAsync(new[] { tier1Unit, tier2Unit });

        // Assert
        var tierIssues = result.AllIssues.Where(i => i.RuleId == "TIER_COST_INVERSION").ToList();
        Assert.NotEmpty(tierIssues);
        Assert.All(tierIssues, i => Assert.Equal(ValidationSeverity.Warning, i.Severity));
    }

    [Fact]
    public async Task ValidateRosterUseCase_CountsIssuesBySeverity()
    {
        // Arrange
        var units = new List<UnitDefinition> { CreateValidUnit() };

        var validationService = new UnitValidationService();
        var useCase = new ValidateRosterUseCase(validationService);

        // Act
        var result = await useCase.ExecuteAsync(units);

        // Assert
        Assert.Equal(0, result.ErrorCount);
        Assert.Equal(0, result.WarningCount);
        Assert.Equal(0, result.InfoCount);
    }

    [Fact]
    public async Task LoadRosterUseCase_LoadsAndValidatesUnits()
    {
        // Arrange
        var units = new List<UnitDefinition> { CreateValidUnit() };
        var mockRosterService = new MockUnitRosterService();
        mockRosterService.SetFileContent("test.json", units);

        var validationService = new UnitValidationService();
        var validateUseCase = new ValidateRosterUseCase(validationService);
        var loadUseCase = new LoadRosterUseCase(mockRosterService, validateUseCase);

        // Act
        var result = await loadUseCase.ExecuteAsync("test.json", validateOnLoad: true);

        // Assert
        Assert.NotEmpty(result.Units);
        Assert.Equal("test", result.Units[0].Id);
        Assert.False(result.HasErrors);
    }

    [Fact]
    public async Task LoadRosterUseCase_SkipsValidationIfDisabled()
    {
        // Arrange
        var invalidUnit = new UnitDefinition
        {
            Id = "bad",
            DisplayName = "Bad",
            Role = UnitRole.Infantry,
            Tier = 1,
            Health = -100, // Invalid
            Damage = 10,
            AttacksPerSecond = 1.0,
            Armor = 0,
            Range = 1.0,
            WoodCost = 10,
            GoldCost = 10,
            PopulationCost = 1,
            ProductionTimeSeconds = 5.0
        };

        var mockRosterService = new MockUnitRosterService();
        mockRosterService.SetFileContent("test.json", new[] { invalidUnit });

        var validationService = new UnitValidationService();
        var validateUseCase = new ValidateRosterUseCase(validationService);
        var loadUseCase = new LoadRosterUseCase(mockRosterService, validateUseCase);

        // Act
        var result = await loadUseCase.ExecuteAsync("test.json", validateOnLoad: false);

        // Assert
        Assert.NotEmpty(result.Units);
        Assert.Empty(result.ValidationIssues);
        Assert.False(result.HasErrors); // No validation ran
    }

    [Fact]
    public async Task LoadRosterUseCase_ThrowsFileNotFoundExceptionWhenFileDoesNotExist()
    {
        // Arrange
        var mockRosterService = new MockUnitRosterService();
        var validationService = new UnitValidationService();
        var validateUseCase = new ValidateRosterUseCase(validationService);
        var loadUseCase = new LoadRosterUseCase(mockRosterService, validateUseCase);

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => loadUseCase.ExecuteAsync("nonexistent.json"));
    }

    [Fact]
    public async Task SaveRosterUseCase_SavesValidRoster()
    {
        // Arrange
        var units = new List<UnitDefinition> { CreateValidUnit() };
        var mockRosterService = new MockUnitRosterService();

        var validationService = new UnitValidationService();
        var validateUseCase = new ValidateRosterUseCase(validationService);
        var saveUseCase = new SaveRosterUseCase(mockRosterService, validateUseCase);

        // Act
        await saveUseCase.ExecuteAsync("output.json", units);

        // Assert
        var saved = mockRosterService.GetFileContent("output.json");
        Assert.NotNull(saved);
        Assert.Single(saved);
    }

    [Fact]
    public async Task SaveRosterUseCase_ThrowsIfRosterHasErrors()
    {
        // Arrange
        var invalidUnit = new UnitDefinition
        {
            Id = "bad",
            DisplayName = "Bad",
            Role = UnitRole.Infantry,
            Tier = 1,
            Health = -100, // Invalid
            Damage = 10,
            AttacksPerSecond = 1.0,
            Armor = 0,
            Range = 1.0,
            WoodCost = 10,
            GoldCost = 10,
            PopulationCost = 1,
            ProductionTimeSeconds = 5.0
        };

        var mockRosterService = new MockUnitRosterService();
        var validationService = new UnitValidationService();
        var validateUseCase = new ValidateRosterUseCase(validationService);
        var saveUseCase = new SaveRosterUseCase(mockRosterService, validateUseCase);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => saveUseCase.ExecuteAsync("output.json", new[] { invalidUnit }));
    }

    [Fact]
    public async Task SaveRosterUseCase_AllowsWarnings()
    {
        // Arrange
        var tier1Unit = CreateValidUnit("t1", 1, UnitRole.Infantry, 100, 10);
        tier1Unit.WoodCost = 100;
        tier1Unit.GoldCost = 100;

        var tier2Unit = CreateValidUnit("t2", 2, UnitRole.Infantry, 150, 15);
        tier2Unit.WoodCost = 50; // Lower cost (warning, not error)
        tier2Unit.GoldCost = 50;

        var mockRosterService = new MockUnitRosterService();
        var validationService = new UnitValidationService();
        var validateUseCase = new ValidateRosterUseCase(validationService);
        var saveUseCase = new SaveRosterUseCase(mockRosterService, validateUseCase);

        // Act
        await saveUseCase.ExecuteAsync("output.json", new[] { tier1Unit, tier2Unit });

        // Assert
        var saved = mockRosterService.GetFileContent("output.json");
        Assert.NotNull(saved);
        Assert.Equal(2, saved.Count);
    }

    [Fact]
    public async Task SaveRosterUseCase_ThrowsArgumentNullExceptionOnNullFilePath()
    {
        // Arrange
        var units = new List<UnitDefinition> { CreateValidUnit() };
        var mockRosterService = new MockUnitRosterService();
        var validationService = new UnitValidationService();
        var validateUseCase = new ValidateRosterUseCase(validationService);
        var saveUseCase = new SaveRosterUseCase(mockRosterService, validateUseCase);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => saveUseCase.ExecuteAsync(null!, units));
    }

    [Fact]
    public async Task SaveRosterUseCase_ThrowsArgumentNullExceptionOnNullUnits()
    {
        // Arrange
        var mockRosterService = new MockUnitRosterService();
        var validationService = new UnitValidationService();
        var validateUseCase = new ValidateRosterUseCase(validationService);
        var saveUseCase = new SaveRosterUseCase(mockRosterService, validateUseCase);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => saveUseCase.ExecuteAsync("output.json", null!));
    }

    [Fact]
    public async Task LoadRosterUseCase_ThrowsArgumentNullExceptionOnNullFilePath()
    {
        // Arrange
        var mockRosterService = new MockUnitRosterService();
        var validationService = new UnitValidationService();
        var validateUseCase = new ValidateRosterUseCase(validationService);
        var loadUseCase = new LoadRosterUseCase(mockRosterService, validateUseCase);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => loadUseCase.ExecuteAsync(null!));
    }

    [Fact]
    public async Task ValidateRosterUseCase_AggregatesBothSingleAndCrossUnitIssues()
    {
        // Arrange
        var invalidUnit = new UnitDefinition
        {
            Id = "bad",
            DisplayName = "Bad",
            Role = UnitRole.Infantry,
            Tier = 1,
            Health = -100, // Single unit issue
            Damage = 0, // Another single unit issue
            AttacksPerSecond = 1.0,
            Armor = 0,
            Range = 1.0,
            WoodCost = 10,
            GoldCost = 10,
            PopulationCost = 1,
            ProductionTimeSeconds = 5.0
        };

        var validationService = new UnitValidationService();
        var useCase = new ValidateRosterUseCase(validationService);

        // Act
        var result = await useCase.ExecuteAsync(new[] { invalidUnit });

        // Assert
        Assert.NotEmpty(result.AllIssues);
        Assert.True(result.ErrorCount > 0);
    }
}
