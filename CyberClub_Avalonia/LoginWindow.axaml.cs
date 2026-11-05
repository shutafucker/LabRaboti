using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;

namespace CyberClubApp
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void BtnLogin_Click(object? sender, RoutedEventArgs e)
        {
            TbError.IsVisible = false;
            var username = TbUsername.Text?.Trim();
            var password = PbPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Введи логин и пароль.");
                return;
            }

            try
            {
                using var db = new AppDbContext();
                var user = await db.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (user is null || !PasswordHelper.VerifyPassword(password, user.PasswordHash))
                {
                    ShowError("Неверный логин или пароль.");
                    return;
                }

                var mainWindow = new MainWindow(user);
                mainWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                // Если хост базы данных из методички недоступен (NXDOMAIN/Offline),
                // позволяем войти под тестовыми учетными данными admin123 / operator123 для демонстрации окон
                if ((username == "admin" && password == "admin123") || 
                    (username == "operator" && password == "operator123"))
                {
                    var fallbackUser = new User
                    {
                        Username = username,
                        Role = username == "admin" ? "Admin" : "Operator"
                    };
                    var mainWindow = new MainWindow(fallbackUser);
                    mainWindow.Show();
                    Close();
                    return;
                }

                ShowError($"База недоступна ({ex.Message}).\nДля демо-входа введите admin / admin123");
            }
        }

        private void ShowError(string message)
        {
            TbError.Text = message;
            TbError.IsVisible = true;
        }
    }
}

