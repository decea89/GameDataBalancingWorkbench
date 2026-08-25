namespace BalanceForge.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>
/// Represents one selectable value in a role or tier filter.
/// </summary>
public partial class FilterOptionViewModel<T> : ObservableObject
    where T : notnull
{
    public FilterOptionViewModel(T value, bool isSelected = true)
    {
        Value = value;
        this.isSelected = isSelected;
    }

    public T Value { get; }

    [ObservableProperty]
    private bool isSelected;
}
