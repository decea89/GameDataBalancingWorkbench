namespace BalanceForge.Desktop.Converters;

using System.Globalization;
using System.Windows;
using System.Windows.Data;

/// <summary>
/// Converts non-empty string to Visibility.Visible, empty string to Visibility.Collapsed.
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str && !string.IsNullOrEmpty(str))
            return Visibility.Visible;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts non-empty string to true, empty string to false.
/// </summary>
public class NotEmptyToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is string str && !string.IsNullOrEmpty(str);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts true to Collapsed and false to Visible.
/// </summary>
public class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not Visibility.Visible;
    }
}
