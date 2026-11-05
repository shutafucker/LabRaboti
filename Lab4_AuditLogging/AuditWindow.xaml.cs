using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;

namespace CyberClubApp
{
    public partial class AuditWindow : Window
    {
        public AuditWindow()
        {
            InitializeComponent();
            Loaded += async (_, __) => await LoadAuditAsync();
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
                MessageBox.Show($"Ошибка загрузки аудита: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e) => await LoadAuditAsync();
        private async void Filter_Changed(object sender, RoutedEventArgs e) => await LoadAuditAsync();
    }
}

