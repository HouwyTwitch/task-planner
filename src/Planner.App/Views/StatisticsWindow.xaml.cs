using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using Planner.App.ViewModels;

namespace Planner.App.Views;

public partial class StatisticsWindow : Window
{
    public StatisticsWindow() => InitializeComponent();

    private void Export_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not StatisticsViewModel vm) return;

        var dialog = new SaveFileDialog
        {
            Title = "Сохранить отчёт",
            Filter = "CSV для Excel (*.csv)|*.csv",
            FileName = $"Статистика {vm.From:yyyy-MM-dd} — {vm.To:yyyy-MM-dd}.csv",
            AddExtension = true,
            DefaultExt = ".csv"
        };
        if (dialog.ShowDialog(this) != true) return;

        try
        {
            // UTF-8 с BOM: Excel открывает кириллицу без мастера импорта.
            File.WriteAllText(dialog.FileName, vm.BuildCsv(), new UTF8Encoding(true));
            MessageBox.Show(this, $"Отчёт сохранён:\n{dialog.FileName}", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Не удалось сохранить отчёт", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
