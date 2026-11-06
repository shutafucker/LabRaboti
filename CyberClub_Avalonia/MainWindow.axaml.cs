using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;

namespace CyberClubApp
{
    public partial class MainWindow : Window
    {
        private readonly User _currentUser;
        private List<Computer> _allComputers = new();

        public MainWindow() : this(new User { Username = "admin", Role = "Admin" })
        {
        }

        public MainWindow(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;

            Opened += async (_, __) =>
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
            BtnAddComputer.IsEnabled = isAdmin;
            BtnDeleteComputer.IsEnabled = false;

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
                Console.WriteLine($"Ошибка соединения: {ex.Message}");
                if (_allComputers.Count == 0)
                {
                    _allComputers = new List<Computer>
                    {
                        new() { Id = 1, Name = "PC-01 (VIP)", IsVip = true, Sessions = new() { new Session { Id = 1, GamerName = "Neo", StartTime = DateTime.UtcNow.AddMinutes(-42), ComputerId = 1 } } },
                        new() { Id = 2, Name = "PC-02", IsVip = false },
                        new() { Id = 3, Name = "PC-03", IsVip = false, Sessions = new() { new Session { Id = 2, GamerName = "Trinity", StartTime = DateTime.UtcNow.AddMinutes(-15), ComputerId = 3 } } },
                        new() { Id = 4, Name = "PC-04 (VIP)", IsVip = true }
                    };
                    ApplyFilters();
                }
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

        private void TbSearch_TextChanged(object? sender, TextChangedEventArgs e) => ApplyFilters();
        private void Filter_Changed(object? sender, RoutedEventArgs e) => ApplyFilters();

        // ---------- ВЫБОР ПК → ПОКАЗ СЕССИЙ ----------
        private void DgComputers_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            bool isAdmin = string.Equals(_currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase);
            if (DgComputers.SelectedItem is Computer pc)
            {
                DgSessions.ItemsSource = pc.Sessions
                    .OrderByDescending(s => s.StartTime)
                    .ToList();
                BtnDeleteComputer.IsEnabled = isAdmin && !pc.IsOccupied;
            }
            else
            {
                DgSessions.ItemsSource = null;
                BtnDeleteComputer.IsEnabled = false;
            }
        }

        // ---------- ДОБАВИТЬ ПК ----------
        private async void BtnAddComputer_Click(object? sender, RoutedEventArgs e)
        {
            if (!string.Equals(_currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                return;

            var dialog = new AddComputerWindow(_currentUser);
            var newComputer = await dialog.ShowDialog<Computer?>(this);
            if (newComputer != null)
            {
                await LoadComputersAsync();
                SelectComputerById(newComputer.Id);
            }
        }

        // ---------- УДАЛИТЬ ПК ----------
        private async void BtnDeleteComputer_Click(object? sender, RoutedEventArgs e)
        {
            if (!string.Equals(_currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                return;

            if (DgComputers.SelectedItem is not Computer pc)
                return;

            if (pc.IsOccupied)
                return;

            try
            {
                using var db = CreateDbContext();
                var dbComp = await db.Computers.FindAsync(pc.Id);
                if (dbComp != null)
                {
                    db.Computers.Remove(dbComp);
                    await db.SaveChangesAsync();
                }

                await LoadComputersAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления ПК: {ex.Message}");
                _allComputers.Remove(pc);
                ApplyFilters();
            }
        }

        // ---------- ПОСАДИТЬ ИГРОКА ----------
        private async void BtnSeat_Click(object? sender, RoutedEventArgs e)
        {
            if (!string.Equals(_currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                return;

            if (DgComputers.SelectedItem is not Computer pc)
                return;

            var name = TbGamerName.Text?.Trim();
            if (string.IsNullOrEmpty(name))
                return;

            if (pc.IsOccupied)
                return;

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

                TbGamerName.Text = string.Empty;
                await LoadComputersAsync();
                SelectComputerById(pc.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка посадки: {ex.Message}");
                var newId = (pc.Sessions.Any() ? pc.Sessions.Max(s => s.Id) : 0) + 10;
                pc.Sessions.Add(new Session
                {
                    Id = newId,
                    GamerName = name,
                    StartTime = DateTime.UtcNow,
                    ComputerId = pc.Id
                });
                TbGamerName.Text = string.Empty;
                ApplyFilters();
                SelectComputerById(pc.Id);
                DgSessions.ItemsSource = pc.Sessions.OrderByDescending(s => s.StartTime).ToList();
            }
        }

        // ---------- ЗАКРЫТЬ СЕССИЮ ----------
        private async void BtnCloseSession_Click(object? sender, RoutedEventArgs e)
        {
            if (!string.Equals(_currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                return;

            if (DgSessions.SelectedItem is not Session s || !s.IsActive)
                return;

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
                Console.WriteLine($"Ошибка закрытия: {ex.Message}");
                s.EndTime = DateTime.UtcNow;
                ApplyFilters();
                SelectComputerById(s.ComputerId);
                if (DgComputers.SelectedItem is Computer pc)
                    DgSessions.ItemsSource = pc.Sessions.OrderByDescending(x => x.StartTime).ToList();
            }
        }

        // ---------- ЖУРНАЛ АУДИТА ----------
        private async void BtnAudit_Click(object? sender, RoutedEventArgs e)
        {
            var auditWindow = new AuditWindow();
            await auditWindow.ShowDialog(this);
        }

        // ---------- ОКНО ОТЧЁТОВ ----------
        private async void BtnReports_Click(object? sender, RoutedEventArgs e)
        {
            var reportsWindow = new ReportsWindow();
            await reportsWindow.ShowDialog(this);
        }

        // ---------- ВЫХОД ИЗ СИСТЕМЫ ----------
        private void BtnLogout_Click(object? sender, RoutedEventArgs e)
        {
            var login = new LoginWindow();
            login.Show();
            Close();
        }

        // ---------- ВСПОМОГАТЕЛЬНОЕ ----------
        private void SelectComputerById(int id)
        {
            var match = (DgComputers.ItemsSource as IEnumerable<Computer>)?
                .FirstOrDefault(c => c.Id == id);
            if (match != null)
            {
                DgComputers.SelectedItem = match;
            }
        }
    }
}

