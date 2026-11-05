using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;

namespace CyberClubApp
{
    public partial class MainWindow : Window
    {
        private readonly User _currentUser;
        private List<Computer> _allComputers = new();

        public MainWindow(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;

            Loaded += async (_, __) =>
            {
                ApplyRolePermissions();
                await LoadComputersAsync();
            };
        }

        private AppDbContext CreateDbContext()
        {
            var db = new AppDbContext();
            db.SetCurrentUser(_currentUser.Username);
            return db;
        }

        private void ApplyRolePermissions()
        {
            bool isAdmin = string.Equals(_currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase);
            BtnSeat.IsEnabled = isAdmin;
            BtnCloseSession.IsEnabled = isAdmin;

            TbCurrentUser.Text = $"Вы вошли как: {_currentUser.Username} ({_currentUser.Role})";
        }

        // ---------- ЗАГРУЗКА ----------
        private async Task LoadComputersAsync()
        {
            try
            {
                using var db = CreateDbContext();
                _allComputers = await db.Computers
                    .AsNoTracking()
                    .Include(c => c.Sessions)
                    .OrderBy(c => c.Id)
                    .ToListAsync();

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка соединения с Supabase: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ---------- ФИЛЬТРАЦИЯ ----------
        private void ApplyFilters()
        {
            var query = _allComputers.AsEnumerable();

            var search = TbSearch.Text?.Trim() ?? "";
            if (search.Length > 0)
                query = query.Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

            if (CbOnlyFree.IsChecked == true)
                query = query.Where(c => !c.IsOccupied);

            if (CbOnlyVip.IsChecked == true)
                query = query.Where(c => c.IsVip);

            DgComputers.ItemsSource = query.ToList();
        }

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void Filter_Changed(object sender, RoutedEventArgs e) => ApplyFilters();

        // ---------- ВЫБОР ПК → ПОКАЗ СЕССИЙ ----------
        private void DgComputers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgComputers.SelectedItem is Computer pc)
                DgSessions.ItemsSource = pc.Sessions
                    .OrderByDescending(s => s.StartTime)
                    .ToList();
            else
                DgSessions.ItemsSource = null;
        }

        // ---------- ПОСАДИТЬ ИГРОКА ----------
        private async void BtnSeat_Click(object sender, RoutedEventArgs e)
        {
            if (!string.Equals(_currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Действие запрещено: у вашей учетной записи нет прав администратора.",
                    "Отказ в доступе", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DgComputers.SelectedItem is not Computer pc)
            {
                MessageBox.Show("Сначала выбери ПК в левой таблице.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var name = TbGamerName.Text?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введи имя игрока.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (pc.IsOccupied)
            {
                MessageBox.Show($"За {pc.Name} уже кто-то играет. Закрой сессию сначала.",
                    "Занято", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var db = CreateDbContext();
                db.Sessions.Add(new Session
                {
                    GamerName = name,
                    StartTime = DateTime.UtcNow,
                    ComputerId = pc.Id
                });
                await db.SaveChangesAsync();

                TbGamerName.Clear();
                await LoadComputersAsync();
                SelectComputerById(pc.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка посадки: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ---------- ЗАКРЫТЬ СЕССИЮ ----------
        private async void BtnCloseSession_Click(object sender, RoutedEventArgs e)
        {
            if (!string.Equals(_currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Действие запрещено: у вашей учетной записи нет прав администратора.",
                    "Отказ в доступе", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DgSessions.SelectedItem is not Session s)
            {
                MessageBox.Show("Выбери сессию в правой таблице.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!s.IsActive)
            {
                MessageBox.Show("Эта сессия уже закрыта.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                using var db = CreateDbContext();
                var dbSession = await db.Sessions.FindAsync(s.Id);
                if (dbSession is null) return;

                dbSession.EndTime = DateTime.UtcNow;
                await db.SaveChangesAsync();

                var pcId = s.ComputerId;
                await LoadComputersAsync();
                SelectComputerById(pcId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка закрытия: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ---------- ЖУРНАЛ АУДИТА ----------
        private void BtnAudit_Click(object sender, RoutedEventArgs e)
        {
            new AuditWindow { Owner = this }.ShowDialog();
        }

        // ---------- ВЫХОД ИЗ СИСТЕМЫ ----------
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var appPath = Environment.ProcessPath ?? Assembly.GetExecutingAssembly().Location;
            Process.Start(appPath);
            Application.Current.Shutdown();
        }

        // ---------- ВСПОМОГАТЕЛЬНОЕ ----------
        private void SelectComputerById(int id)
        {
            var match = (DgComputers.ItemsSource as IEnumerable<Computer>)?
                .FirstOrDefault(c => c.Id == id);
            if (match != null)
            {
                DgComputers.SelectedItem = match;
                DgComputers.ScrollIntoView(match);
            }
        }
    }
}

