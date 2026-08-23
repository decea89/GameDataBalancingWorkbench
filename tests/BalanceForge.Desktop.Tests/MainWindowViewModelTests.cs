namespace BalanceForge.Desktop.Tests;

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

        var viewModel = new MainWindowViewModel(mockFileDialog.Object, mockLoadUseCase.Object);

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

        var viewModel = new MainWindowViewModel(mockFileDialog.Object, mockLoadUseCase.Object);

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

        var viewModel = new MainWindowViewModel(mockFileDialog.Object, mockLoadUseCase.Object);
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

        var viewModel = new MainWindowViewModel(mockFileDialog.Object, mockLoadUseCase.Object);
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

        mockLoadUseCase
            .Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("File not found"));

        var viewModel = new MainWindowViewModel(mockFileDialog.Object, mockLoadUseCase.Object);
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

        var tcs = new TaskCompletionSource<RosterLoadResult>();
        mockLoadUseCase
            .Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        var viewModel = new MainWindowViewModel(mockFileDialog.Object, mockLoadUseCase.Object);
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
}
