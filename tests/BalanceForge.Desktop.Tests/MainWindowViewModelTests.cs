namespace BalanceForge.Desktop.Tests;

using BalanceForge.Application;
using BalanceForge.Application.Results;
using BalanceForge.Application.UseCases;
using BalanceForge.Desktop.Services;
using BalanceForge.Desktop.ViewModels;
using BalanceForge.Domain;
using Moq;

public class MainWindowViewModelTests
{
    [Fact]
    public async Task SelectFile_WithValidPath_SetsSelectedFilePath()
    {
        // Arrange
        var mockFileDialog = new Mock<IFileDialogService>();
        mockFileDialog
            .Setup(x => x.OpenFileAsync(It.IsAny<string>()))
            .ReturnsAsync("/path/to/units.json");

        var mockLoadUseCase = new Mock<ILoadRosterUseCase>();
        var calculator = new BalanceMetricsCalculator();

        var viewModel = new MainWindowViewModel(mockFileDialog.Object, mockLoadUseCase.Object, calculator);

        // Act
        await viewModel.SelectFileCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal("/path/to/units.json", viewModel.SelectedFilePath);
        Assert.Empty(viewModel.ErrorMessage);
    }

    [Fact]
    public async Task SelectFile_WhenCancelled_DoesNotSetFilePath()
    {
        // Arrange
        var mockFileDialog = new Mock<IFileDialogService>();
        mockFileDialog
            .Setup(x => x.OpenFileAsync(It.IsAny<string>()))
            .ReturnsAsync((string?)null);

        var mockLoadUseCase = new Mock<ILoadRosterUseCase>();
        var calculator = new BalanceMetricsCalculator();

        var viewModel = new MainWindowViewModel(mockFileDialog.Object, mockLoadUseCase.Object, calculator);

        // Act
        await viewModel.SelectFileCommand.ExecuteAsync(null);

        // Assert
        Assert.Empty(viewModel.SelectedFilePath);
    }

    [Fact]
    public async Task Load_WithNoFilePath_ShowsError()
    {
        // Arrange
        var mockFileDialog = new Mock<IFileDialogService>();
        var mockLoadUseCase = new Mock<ILoadRosterUseCase>();
        var calculator = new BalanceMetricsCalculator();

        var viewModel = new MainWindowViewModel(mockFileDialog.Object, mockLoadUseCase.Object, calculator);
        viewModel.SelectedFilePath = string.Empty;

        // Act
        await viewModel.LoadCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal("Please select a file first.", viewModel.ErrorMessage);
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public async Task Load_WithValidFile_PopulatesUnitsAndStatus()
    {
        // Arrange
        var mockFileDialog = new Mock<IFileDialogService>();
        var mockLoadUseCase = new Mock<ILoadRosterUseCase>();

        var unit = new UnitDefinition
        {
            Id = "warrior",
            DisplayName = "Warrior",
            Role = UnitRole.Infantry,
            Tier = 1,
            Health = 100,
            Damage = 10,
            AttacksPerSecond = 1,
            Armor = 2,
            Range = 1,
            WoodCost = 50,
            GoldCost = 25,
            PopulationCost = 2,
            ProductionTimeSeconds = 10
        };

        var result = new RosterLoadResult(
            Units: new[] { unit }.ToList().AsReadOnly(),
            ValidationIssues: new List<ValidationIssue>(),
            HasErrors: false
        );

        mockLoadUseCase
            .Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        var calculator = new BalanceMetricsCalculator();
        var viewModel = new MainWindowViewModel(mockFileDialog.Object, mockLoadUseCase.Object, calculator);
        viewModel.SelectedFilePath = "/path/to/units.json";

        // Act
        await viewModel.LoadCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(1, viewModel.LoadedUnitCount);
        Assert.Equal(0, viewModel.ValidationIssueCount);
        Assert.Single(viewModel.Units);
        Assert.Empty(viewModel.ErrorMessage);
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public async Task Load_WithLoadErrors_ShowsErrorMessage()
    {
        // Arrange
        var mockFileDialog = new Mock<IFileDialogService>();
        var mockLoadUseCase = new Mock<ILoadRosterUseCase>();
        var calculator = new BalanceMetricsCalculator();

        mockLoadUseCase
            .Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("File not found"));

        var viewModel = new MainWindowViewModel(mockFileDialog.Object, mockLoadUseCase.Object, calculator);
        viewModel.SelectedFilePath = "/nonexistent/path.json";

        // Act
        await viewModel.LoadCommand.ExecuteAsync(null);

        // Assert
        Assert.Contains("Failed to load roster", viewModel.ErrorMessage);
        Assert.Equal(0, viewModel.LoadedUnitCount);
        Assert.Empty(viewModel.Units);
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public async Task Load_SetsIsLoadingDuringOperation()
    {
        // Arrange
        var mockFileDialog = new Mock<IFileDialogService>();
        var mockLoadUseCase = new Mock<ILoadRosterUseCase>();
        var calculator = new BalanceMetricsCalculator();

        var tcs = new TaskCompletionSource<RosterLoadResult>();
        mockLoadUseCase
            .Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        var viewModel = new MainWindowViewModel(mockFileDialog.Object, mockLoadUseCase.Object, calculator);
        viewModel.SelectedFilePath = "/path/to/units.json";

        // Act - start load
        var loadTask = viewModel.LoadCommand.ExecuteAsync(null);
        await Task.Delay(10); // Give command time to set IsLoading

        // Assert during load
        Assert.True(viewModel.IsLoading);

        // Complete load
        tcs.SetResult(new RosterLoadResult(
            Units: new List<UnitDefinition>().AsReadOnly(),
            ValidationIssues: new List<ValidationIssue>(),
            HasErrors: false
        ));

        await loadTask;

        // Assert after load
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public void ApplyFilters_WithRoleFilter_FiltersUnits()
    {
        // Arrange
        var mockFileDialog = new Mock<IFileDialogService>();
        var mockLoadUseCase = new Mock<ILoadRosterUseCase>();
        var calculator = new BalanceMetricsCalculator();

        var unit1 = new UnitDefinition
        {
            Id = "warrior",
            DisplayName = "Warrior",
            Role = UnitRole.Infantry,
            Tier = 1,
            Health = 100,
            Damage = 10,
            AttacksPerSecond = 1,
            Armor = 2,
            Range = 1,
            WoodCost = 50,
            GoldCost = 25,
            PopulationCost = 2,
            ProductionTimeSeconds = 10
        };

        var unit2 = new UnitDefinition
        {
            Id = "archer",
            DisplayName = "Archer",
            Role = UnitRole.Ranged,
            Tier = 1,
            Health = 50,
            Damage = 12,
            AttacksPerSecond = 2,
            Armor = 0,
            Range = 5,
            WoodCost = 40,
            GoldCost = 20,
            PopulationCost = 1,
            ProductionTimeSeconds = 8
        };

        var viewModel = new MainWindowViewModel(mockFileDialog.Object, mockLoadUseCase.Object, calculator);
        viewModel.Units = new System.Collections.ObjectModel.ObservableCollection<UnitDefinition>(new[] { unit1, unit2 });

        // Filter to only Infantry
        viewModel.SelectedRoles = new HashSet<UnitRole> { UnitRole.Infantry };
        viewModel.SelectedTiers = new HashSet<int> { 1 };

        // Act
        viewModel.ApplyFiltersCommand.Execute(null);

        // Assert
        Assert.Single(viewModel.DisplayedUnits);
        Assert.Equal("Warrior", viewModel.DisplayedUnits[0].DisplayName);
    }

    [Fact]
    public void ApplyFilters_WithTierFilter_FiltersUnits()
    {
        // Arrange
        var mockFileDialog = new Mock<IFileDialogService>();
        var mockLoadUseCase = new Mock<ILoadRosterUseCase>();
        var calculator = new BalanceMetricsCalculator();

        var unit1 = new UnitDefinition
        {
            Id = "warrior",
            DisplayName = "Warrior",
            Role = UnitRole.Infantry,
            Tier = 1,
            Health = 100,
            Damage = 10,
            AttacksPerSecond = 1,
            Armor = 2,
            Range = 1,
            WoodCost = 50,
            GoldCost = 25,
            PopulationCost = 2,
            ProductionTimeSeconds = 10
        };

        var unit2 = new UnitDefinition
        {
            Id = "knight",
            DisplayName = "Knight",
            Role = UnitRole.Infantry,
            Tier = 2,
            Health = 150,
            Damage = 15,
            AttacksPerSecond = 1,
            Armor = 3,
            Range = 1,
            WoodCost = 75,
            GoldCost = 50,
            PopulationCost = 3,
            ProductionTimeSeconds = 20
        };

        var viewModel = new MainWindowViewModel(mockFileDialog.Object, mockLoadUseCase.Object, calculator);
        viewModel.Units = new System.Collections.ObjectModel.ObservableCollection<UnitDefinition>(new[] { unit1, unit2 });

        // Filter to only Tier 2
        viewModel.SelectedRoles = new HashSet<UnitRole> { UnitRole.Infantry };
        viewModel.SelectedTiers = new HashSet<int> { 2 };

        // Act
        viewModel.ApplyFiltersCommand.Execute(null);

        // Assert
        Assert.Single(viewModel.DisplayedUnits);
        Assert.Equal("Knight", viewModel.DisplayedUnits[0].DisplayName);
    }

    [Fact]
    public void ApplyFilters_CalculatesMetricsForDisplayedUnits()
    {
        // Arrange
        var mockFileDialog = new Mock<IFileDialogService>();
        var mockLoadUseCase = new Mock<ILoadRosterUseCase>();
        var calculator = new BalanceMetricsCalculator();

        var unit = new UnitDefinition
        {
            Id = "warrior",
            DisplayName = "Warrior",
            Role = UnitRole.Infantry,
            Tier = 1,
            Health = 100,
            Damage = 10,
            AttacksPerSecond = 1,
            Armor = 2,
            Range = 1,
            WoodCost = 50,
            GoldCost = 25,
            PopulationCost = 2,
            ProductionTimeSeconds = 10
        };

        var viewModel = new MainWindowViewModel(mockFileDialog.Object, mockLoadUseCase.Object, calculator);
        viewModel.Units = new System.Collections.ObjectModel.ObservableCollection<UnitDefinition>(new[] { unit });
        viewModel.SelectedRoles = new HashSet<UnitRole> { UnitRole.Infantry };
        viewModel.SelectedTiers = new HashSet<int> { 1 };

        // Act
        viewModel.ApplyFiltersCommand.Execute(null);

        // Assert
        Assert.Single(viewModel.DisplayedUnits);
        var displayedUnit = viewModel.DisplayedUnits[0];
        Assert.Equal(10.0, displayedUnit.DPS); // 10 damage * 1 attack/sec
        Assert.Equal(75.0, displayedUnit.TotalCost); // 50 + 25
        Assert.True(displayedUnit.DPSPerCost > 0);
    }
}
