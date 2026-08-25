namespace BalanceForge.Desktop.ViewModels;

using System.Collections.ObjectModel;
using BalanceForge.Domain;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

/// <summary>
/// View model for the validation issues panel.
/// Displays and filters validation issues by severity and affected unit.
/// </summary>
public partial class IssuesPanelViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ValidationIssue> displayedIssues = new();

    [ObservableProperty]
    private int displayedIssueCount;

    [ObservableProperty]
    private ObservableCollection<ValidationSeverity> severityFilters = new(new[]
    {
        ValidationSeverity.Info,
        ValidationSeverity.Warning,
        ValidationSeverity.Error
    });

    [ObservableProperty]
    private ObservableCollection<string> unitIdFilters = new();

    [ObservableProperty]
    private ValidationIssue? selectedIssue;

    private ObservableCollection<ValidationIssue> _allIssues = new();

    public void UpdateIssues(IEnumerable<ValidationIssue> issues)
    {
        _allIssues = new ObservableCollection<ValidationIssue>(issues);
        ApplyFilters();
    }

    public void AddSeverityFilter(ValidationSeverity severity)
    {
        if (!SeverityFilters.Contains(severity))
        {
            SeverityFilters.Add(severity);
        }
    }

    public void RemoveSeverityFilter(ValidationSeverity severity)
    {
        SeverityFilters.Remove(severity);
        ApplyFilters();
    }

    public void AddUnitIdFilter(string unitId)
    {
        if (!UnitIdFilters.Contains(unitId))
        {
            UnitIdFilters.Add(unitId);
        }
    }

    public void RemoveUnitIdFilter(string unitId)
    {
        UnitIdFilters.Remove(unitId);
        ApplyFilters();
    }

    [RelayCommand]
    public void ApplyFilters()
    {
        var filtered = _allIssues
            .Where(issue => SeverityFilters.Contains(issue.Severity))
            .Where(issue => UnitIdFilters.Count == 0 || UnitIdFilters.Contains(issue.UnitId))
            .ToList();

        DisplayedIssues = new ObservableCollection<ValidationIssue>(filtered);
        DisplayedIssueCount = filtered.Count;
    }

    [RelayCommand]
    public void ClearFilters()
    {
        SeverityFilters = new ObservableCollection<ValidationSeverity>(new[]
        {
            ValidationSeverity.Info,
            ValidationSeverity.Warning,
            ValidationSeverity.Error
        });
        UnitIdFilters.Clear();
        ApplyFilters();
    }
}
