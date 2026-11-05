using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace CyberClubApp
{
    public partial class LoginWindow : Window
    {
        // Сюда сохраним вошедшего пользователя, чтобы главное окно знало роль
        public User? LoggedInUser { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            TbError.Visibility = Visibility.Collapsed;
            var username = TbUsername.Text?.Trim();
            var password = PbPassword.Password;

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

                LoggedInUser = user;
                DialogResult = true;   // это закроет окно и вернёт true вызывающему
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка соединения: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            TbError.Text = message;
            TbError.Visibility = Visibility.Visible;
        }
    }
}

