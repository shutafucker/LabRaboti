using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;

namespace CyberClubApp
{
    public partial class AddComputerWindow : Window
    {
        private readonly User _currentUser;

        public AddComputerWindow() : this(new User { Username = "admin", Role = "Admin" })
        {
        }

        public AddComputerWindow(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;

            Opened += (_, __) =>
            {
                TbName.Focus();
            };

            TbName.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    BtnSave_Click(sender, e);
                }
            };
        }

        private void BtnCancel_Click(object? sender, RoutedEventArgs e)
        {
            Close(null);
        }

        private async void BtnSave_Click(object? sender, RoutedEventArgs e)
        {
            var name = TbName.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                TbError.Text = "Введите название компьютера!";
                TbError.IsVisible = true;
                return;
            }

            TbError.IsVisible = false;
            BtnSave.IsEnabled = false;

            try
            {
                using var db = new AppDbContext();
                db.SetCurrentUser(_currentUser.Username);

                bool exists = await db.Computers.AnyAsync(c => c.Name.ToLower() == name.ToLower());
                if (exists)
                {
                    TbError.Text = "Компьютер с таким названием уже существует!";
                    TbError.IsVisible = true;
                    BtnSave.IsEnabled = true;
                    return;
                }

                var computer = new Computer
                {
                    Name = name,
                    IsVip = CbIsVip.IsChecked == true
                };

                db.Computers.Add(computer);
                await db.SaveChangesAsync();

                Close(computer);
            }
            catch (Exception ex)
            {
                TbError.Text = $"Ошибка сохранения: {ex.Message}";
                TbError.IsVisible = true;
                BtnSave.IsEnabled = true;
            }
        }
    }
}

