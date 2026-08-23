namespace BalanceForge.Infrastructure.Tests;

using System.Text.Json;
using BalanceForge.Application.Services;
using BalanceForge.Domain;
using BalanceForge.Infrastructure.Services;
using Xunit;

public class FileBasedUnitRosterServiceTests
{
    private class MockFileAccessor : IFileAccessor
    {
        private readonly Dictionary<string, string> _fileSystem = new();

        public async Task<string> ReadAllTextAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (!_fileSystem.TryGetValue(filePath, out var content))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            return await Task.FromResult(content);
        }

        public async Task WriteAllTextAsync(string filePath, string content, CancellationToken cancellationToken = default)
        {
            _fileSystem[filePath] = content;
            await Task.CompletedTask;
        }

        public bool FileExists(string filePath)
        {
            return _fileSystem.ContainsKey(filePath);
        }

        public void WriteFile(string filePath, string content)
        {
            _fileSystem[filePath] = content;
        }

        public string? ReadFile(string filePath)
        {
            _fileSystem.TryGetValue(filePath, out var content);
            return content;
        }
    }

    private static UnitDefinition CreateValidUnit(
        string id = "test-unit",
        int tier = 1,
        UnitRole role = UnitRole.Infantry,
        double health = 100,
        double damage = 10,
        int woodCost = 10,
        int goldCost = 10)
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
            WoodCost = woodCost,
            GoldCost = goldCost,
            PopulationCost = 1,
            ProductionTimeSeconds = 5.0,
            AllowCostTierInversion = false
        };
    }

    [Fact]
    public async Task LoadAsync_DeserializesValidJsonFile()
    {
        // Arrange
        var json = """
            {
              "units": [
                {
                  "id": "knight",
                  "displayName": "Knight",
                  "role": "Cavalry",
                  "tier": 2,
                  "health": 180.0,
                  "damage": 18.0,
                  "attacksPerSecond": 1.1,
                  "armor": 4.0,
                  "range": 1.5,
                  "woodCost": 0,
                  "goldCost": 90,
                  "populationCost": 2,
                  "productionTimeSeconds": 28.0
                }
              ]
            }
            """;

        var mockFile = new MockFileAccessor();
        mockFile.WriteFile("units.json", json);
        var service = new FileBasedUnitRosterService(mockFile);

        // Act
        var units = await service.LoadAsync("units.json");

        // Assert
        Assert.Single(units);
        var unit = units[0];
        Assert.Equal("knight", unit.Id);
        Assert.Equal("Knight", unit.DisplayName);
        Assert.Equal(UnitRole.Cavalry, unit.Role);
        Assert.Equal(2, unit.Tier);
        Assert.Equal(180.0, unit.Health);
        Assert.Equal(18.0, unit.Damage);
        Assert.Equal(1.1, unit.AttacksPerSecond);
    }

    [Fact]
    public async Task LoadAsync_ThrowsFileNotFoundExceptionWhenFileDoesNotExist()
    {
        // Arrange
        var mockFile = new MockFileAccessor();
        var service = new FileBasedUnitRosterService(mockFile);

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => service.LoadAsync("nonexistent.json"));
    }

    [Fact]
    public async Task LoadAsync_ThrowsInvalidOperationExceptionOnMalformedJson()
    {
        // Arrange
        var mockFile = new MockFileAccessor();
        mockFile.WriteFile("bad.json", "{ this is not valid json }");
        var service = new FileBasedUnitRosterService(mockFile);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.LoadAsync("bad.json"));
    }

    [Fact]
    public async Task LoadAsync_ThrowsInvalidOperationExceptionOnUnknownRole()
    {
        // Arrange
        var json = """
            {
              "units": [
                {
                  "id": "bad",
                  "displayName": "Bad",
                  "role": "UnknownRole",
                  "tier": 1,
                  "health": 100.0,
                  "damage": 10.0,
                  "attacksPerSecond": 1.0,
                  "armor": 0.0,
                  "range": 1.0,
                  "woodCost": 10,
                  "goldCost": 10,
                  "populationCost": 1,
                  "productionTimeSeconds": 5.0
                }
              ]
            }
            """;

        var mockFile = new MockFileAccessor();
        mockFile.WriteFile("bad-role.json", json);
        var service = new FileBasedUnitRosterService(mockFile);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.LoadAsync("bad-role.json"));
    }

    [Fact]
    public async Task LoadAsync_LoadsMultipleUnits()
    {
        // Arrange
        var json = """
            {
              "units": [
                {
                  "id": "unit1",
                  "displayName": "Unit 1",
                  "role": "Infantry",
                  "tier": 1,
                  "health": 100.0,
                  "damage": 10.0,
                  "attacksPerSecond": 1.0,
                  "armor": 0.0,
                  "range": 1.0,
                  "woodCost": 10,
                  "goldCost": 10,
                  "populationCost": 1,
                  "productionTimeSeconds": 5.0
                },
                {
                  "id": "unit2",
                  "displayName": "Unit 2",
                  "role": "Ranged",
                  "tier": 2,
                  "health": 50.0,
                  "damage": 8.0,
                  "attacksPerSecond": 1.5,
                  "armor": 0.0,
                  "range": 5.0,
                  "woodCost": 20,
                  "goldCost": 20,
                  "populationCost": 1,
                  "productionTimeSeconds": 8.0
                }
              ]
            }
            """;

        var mockFile = new MockFileAccessor();
        mockFile.WriteFile("roster.json", json);
        var service = new FileBasedUnitRosterService(mockFile);

        // Act
        var units = await service.LoadAsync("roster.json");

        // Assert
        Assert.Equal(2, units.Count);
        Assert.Equal("unit1", units[0].Id);
        Assert.Equal("unit2", units[1].Id);
    }

    [Fact]
    public async Task SaveAsync_SerializesUnitsToJsonFile()
    {
        // Arrange
        var units = new List<UnitDefinition>
        {
            CreateValidUnit("knight", 2, UnitRole.Cavalry, 180, 18, 0, 90)
        };

        var mockFile = new MockFileAccessor();
        var service = new FileBasedUnitRosterService(mockFile);

        // Act
        await service.SaveAsync("output.json", units);

        // Assert
        var savedJson = mockFile.ReadFile("output.json");
        Assert.NotNull(savedJson);
        Assert.Contains("\"id\": \"knight\"", savedJson);
        Assert.Contains("\"displayName\"", savedJson);
        Assert.Contains("\"role\": \"Cavalry\"", savedJson);
    }

    [Fact]
    public async Task SaveAsync_ThrowsArgumentNullExceptionOnNullFilePath()
    {
        // Arrange
        var units = new List<UnitDefinition> { CreateValidUnit() };
        var mockFile = new MockFileAccessor();
        var service = new FileBasedUnitRosterService(mockFile);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.SaveAsync(null!, units));
    }

    [Fact]
    public async Task SaveAsync_ThrowsArgumentNullExceptionOnNullUnits()
    {
        // Arrange
        var mockFile = new MockFileAccessor();
        var service = new FileBasedUnitRosterService(mockFile);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.SaveAsync("output.json", null!));
    }

    [Fact]
    public async Task SaveAsync_OverwritesExistingFile()
    {
        // Arrange
        var units1 = new List<UnitDefinition> { CreateValidUnit("unit1") };
        var units2 = new List<UnitDefinition> { CreateValidUnit("unit2") };

        var mockFile = new MockFileAccessor();
        var service = new FileBasedUnitRosterService(mockFile);

        // Act
        await service.SaveAsync("output.json", units1);
        var firstWrite = mockFile.ReadFile("output.json");

        await service.SaveAsync("output.json", units2);
        var secondWrite = mockFile.ReadFile("output.json");

        // Assert
        Assert.NotNull(firstWrite);
        Assert.NotNull(secondWrite);
        Assert.Contains("unit1", firstWrite);
        Assert.DoesNotContain("unit1", secondWrite);
        Assert.Contains("unit2", secondWrite);
    }

    [Fact]
    public async Task SaveAsync_PreservesAllUnitProperties()
    {
        // Arrange
        var original = CreateValidUnit("test", 3, UnitRole.Support, 120, 5, 60, 80);
        original.AllowCostTierInversion = true;
        original.AttacksPerSecond = 0.8;

        var units = new List<UnitDefinition> { original };
        var mockFile = new MockFileAccessor();
        var service = new FileBasedUnitRosterService(mockFile);

        // Act
        await service.SaveAsync("output.json", units);
        var loaded = await service.LoadAsync("output.json");

        // Assert
        var unit = loaded[0];
        Assert.Equal("test", unit.Id);
        Assert.Equal(3, unit.Tier);
        Assert.Equal(UnitRole.Support, unit.Role);
        Assert.Equal(120, unit.Health);
        Assert.Equal(5, unit.Damage);
        Assert.Equal(0.8, unit.AttacksPerSecond);
        Assert.Equal(60, unit.WoodCost);
        Assert.Equal(80, unit.GoldCost);
        Assert.True(unit.AllowCostTierInversion);
    }

    [Fact]
    public async Task RoundTrip_PreservesUnitsAfterLoadAndSave()
    {
        // Arrange
        var original = new List<UnitDefinition>
        {
            CreateValidUnit("knight", 2, UnitRole.Cavalry, 180, 18, 0, 90),
            CreateValidUnit("archer", 1, UnitRole.Ranged, 30, 6, 30, 10)
        };

        var mockFile = new MockFileAccessor();
        var service = new FileBasedUnitRosterService(mockFile);

        // Act
        await service.SaveAsync("original.json", original);
        var loaded = await service.LoadAsync("original.json");
        await service.SaveAsync("roundtrip.json", loaded);
        var reloaded = await service.LoadAsync("roundtrip.json");

        // Assert
        Assert.Equal(2, reloaded.Count);
        Assert.Equal("knight", reloaded[0].Id);
        Assert.Equal(UnitRole.Cavalry, reloaded[0].Role);
        Assert.Equal("archer", reloaded[1].Id);
        Assert.Equal(UnitRole.Ranged, reloaded[1].Role);
    }

    [Fact]
    public async Task LoadAsync_ThrowsArgumentNullExceptionOnNullFilePath()
    {
        // Arrange
        var mockFile = new MockFileAccessor();
        var service = new FileBasedUnitRosterService(mockFile);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.LoadAsync(null!));
    }

    [Fact]
    public async Task SaveAsync_SavesEmptyRoster()
    {
        // Arrange
        var units = new List<UnitDefinition>();
        var mockFile = new MockFileAccessor();
        var service = new FileBasedUnitRosterService(mockFile);

        // Act
        await service.SaveAsync("empty.json", units);
        var loaded = await service.LoadAsync("empty.json");

        // Assert
        Assert.Empty(loaded);
    }
}
