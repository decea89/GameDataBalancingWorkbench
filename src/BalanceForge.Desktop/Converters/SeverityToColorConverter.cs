namespace BalanceForge.Desktop.Converters;

using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using BalanceForge.Domain;

/// <summary>
/// Converts ValidationSeverity to Color for UI display.
/// </summary>
public class SeverityToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ValidationSeverity severity)
        {
            return severity switch
            {
                ValidationSeverity.Error => new SolidColorBrush(Colors.Red),
                ValidationSeverity.Warning => new SolidColorBrush(Colors.Orange),
                ValidationSeverity.Info => new SolidColorBrush(Colors.Blue),
                _ => new SolidColorBrush(Colors.Black)
            };
        }

        return new SolidColorBrush(Colors.Black);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
