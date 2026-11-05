using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;

namespace CyberClubApp
{
    public partial class AuditWindow : Window
    {
        public AuditWindow()
        {
            InitializeComponent();
            Opened += async (_, __) => await LoadAuditAsync();
        }

        private async Task LoadAuditAsync()
        {
            try
            {
                using var db = new AppDbContext();
                var query = db.AuditLogs.AsNoTracking().AsQueryable();

                var userFilter = TbFilterUser.Text?.Trim();
                if (!string.IsNullOrEmpty(userFilter))
                    query = query.Where(a => a.ChangedBy != null && a.ChangedBy.Contains(userFilter));

                if (CbFilterTable.SelectedItem is ComboBoxItem item && item.Content?.ToString() != "Все")
                {
                    var table = item.Content?.ToString();
                    query = query.Where(a => a.TableName == table);
                }

                var logs = await query
                    .OrderByDescending(a => a.ChangedAt)
                    .Take(200)
                    .ToListAsync();

                DgAudit.ItemsSource = logs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки аудита: {ex.Message}");
            }
        }

        private async void BtnRefresh_Click(object? sender, RoutedEventArgs e) => await LoadAuditAsync();
        private async void Filter_UserChanged(object? sender, TextChangedEventArgs e) => await LoadAuditAsync();
        private async void Filter_TableChanged(object? sender, SelectionChangedEventArgs e) => await LoadAuditAsync();
    }
}
