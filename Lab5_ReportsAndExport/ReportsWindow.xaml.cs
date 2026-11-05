using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CyberClubApp
{
    public partial class ReportsWindow : Window
    {
        private readonly ReportService _service = new();
        private DateTime _from;
        private DateTime _to;

        public ReportsWindow()
        {
            InitializeComponent();
            Loaded += async (_, __) => await LoadReportAsync();
        }

        private (DateTime from, DateTime to) GetPeriod()
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var selected = (CbPeriod.SelectedItem as ComboBoxItem)?.Content?.ToString();
            return selected switch
            {
                "Сегодня" => (today, today.AddDays(1)),
                "Неделя"  => (today.AddDays(-7), today.AddDays(1)),
                "Месяц"   => (today.AddDays(-30), today.AddDays(1)),
                _         => (today, today.AddDays(1))
            };
        }

        private async System.Threading.Tasks.Task LoadReportAsync()
        {
            try
            {
                (_from, _to) = GetPeriod();
                var summary = await _service.GetSummaryAsync(_from, _to);
                var topPcs = await _service.GetTopPcsAsync(_from, _to);
                var daily = await _service.GetDailyAsync(_from, _to);

                if (summary.TotalSessions == 0)
                {
                    TbSummary.Text = "За выбранный период нет закрытых сессий.";
                    DgTopPcs.ItemsSource = null;
                    DgDaily.ItemsSource = null;
                    return;
                }

                TbSummary.Text =
                    $"Сессий: {summary.TotalSessions}   |   " +
                    $"Всего часов: {summary.TotalHours}   |   " +
                    $"Средняя сессия: {summary.AverageMinutes:F0} мин\n" +
                    $"Самый активный игрок: {summary.TopGamerName} " +
                    $"({summary.TopGamerSessions} сессий, {summary.TopGamerHours} ч)";

                DgTopPcs.ItemsSource = topPcs;
                DgDaily.ItemsSource = daily;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отчёта: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
            => await LoadReportAsync();

        private async void Period_Changed(object sender, RoutedEventArgs e)
            => await LoadReportAsync();

        // ---------- ЭКСПОРТ В CSV ----------
        private async void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var summary = await _service.GetSummaryAsync(_from, _to);
                var topPcs = await _service.GetTopPcsAsync(_from, _to);
                var daily = await _service.GetDailyAsync(_from, _to);

                var sb = new StringBuilder();
                sb.AppendLine("Отчёт CyberArena");
                sb.AppendLine($"Период;{_from:dd.MM.yyyy};{_to.AddDays(-1):dd.MM.yyyy}");
                sb.AppendLine();
                sb.AppendLine("Сводка");
                sb.AppendLine($"Сессий;{summary.TotalSessions}");
                sb.AppendLine($"Всего часов;{summary.TotalHours}");
                sb.AppendLine($"Средняя сессия (мин);{summary.AverageMinutes}");
                sb.AppendLine($"Самый активный игрок;{summary.TopGamerName}");
                sb.AppendLine();
                sb.AppendLine("Топ ПК");
                sb.AppendLine("ПК;VIP;Сессий;Часов");
                foreach (var r in topPcs)
                    sb.AppendLine($"{r.PcName};{(r.IsVip ? "да" : "нет")};{r.SessionCount};{r.TotalHours}");

                sb.AppendLine();
                sb.AppendLine("По дням");
                sb.AppendLine("День;Сессий;Часов");
                foreach (var r in daily)
                    sb.AppendLine($"{r.Day:dd.MM.yyyy};{r.SessionCount};{r.TotalHours}");

                // BOM (UTF-8 with BOM) — чтобы Excel корректно открыл кириллицу
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                if (string.IsNullOrEmpty(desktopPath) || !Directory.Exists(desktopPath))
                {
                    desktopPath = AppDomain.CurrentDomain.BaseDirectory;
                }

                var path = Path.Combine(desktopPath, $"CyberArena_Report_{DateTime.Now:yyyyMMdd_HHmm}.csv");
                File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));

                MessageBox.Show($"Отчёт сохранён:\n{path}",
                    "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

