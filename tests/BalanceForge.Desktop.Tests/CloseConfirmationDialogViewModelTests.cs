namespace BalanceForge.Desktop.Tests;

using BalanceForge.Desktop.ViewModels;
using Xunit;

public class CloseConfirmationDialogViewModelTests
{
    [Fact]
    public void Save_SetsDialogResultToTrue()
    {
        // Arrange
        var viewModel = new CloseConfirmationDialogViewModel();

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert
        Assert.True(viewModel.DialogResult == true);
    }

    [Fact]
    public void Discard_SetsDialogResultToFalse()
    {
        // Arrange
        var viewModel = new CloseConfirmationDialogViewModel();

        // Act
        viewModel.DiscardCommand.Execute(null);

        // Assert
        Assert.False(viewModel.DialogResult == true);
        Assert.True(viewModel.DialogResult == false);
    }

    [Fact]
    public void Cancel_SetsDialogResultToNull()
    {
        // Arrange
        var viewModel = new CloseConfirmationDialogViewModel();

        // Act
        viewModel.CancelCommand.Execute(null);

        // Assert
        Assert.Null(viewModel.DialogResult);
    }
}
