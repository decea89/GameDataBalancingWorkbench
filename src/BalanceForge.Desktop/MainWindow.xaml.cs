using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BalanceForge.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BalanceForge.Desktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var app = (App)System.Windows.Application.Current;
        var viewModel = app.ServiceProvider.GetRequiredService<MainWindowViewModel>();
        DataContext = viewModel;
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Find the DataGrid and attach Ctrl+click handler
        if (FindDataGrid(this) is DataGrid dataGrid)
        {
            dataGrid.PreviewMouseDown += DataGrid_PreviewMouseDown;
        }
    }

    private DataGrid? FindDataGrid(DependencyObject parent)
    {
        if (parent is DataGrid dataGrid)
            return dataGrid;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            var result = FindDataGrid(child);
            if (result != null)
                return result;
        }

        return null;
    }

    private void DataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
            return;

        if (e.OriginalSource is not FrameworkElement element)
            return;

        // Find the row
        var row = FindVisualParent<DataGridRow>(element);
        if (row?.DataContext is not RosterUnitViewModel unit)
            return;

        e.Handled = true;

        if (DataContext is MainWindowViewModel vm)
        {
            vm.SelectUnitForComparisonCommand.Execute(unit);
        }
    }

    private static T? FindVisualParent<T>(DependencyObject? child) where T : DependencyObject
    {
        while (child != null)
        {
            if (child is T parent)
                return parent;

            child = VisualTreeHelper.GetParent(child);
        }

        return null;
    }
}