using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;

namespace CyberClubApp
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var login = new LoginWindow();
                desktop.MainWindow = login;
            }

            base.OnFrameworkInitializationCompleted();

            // Сидирование в фоне, не блокируя запуск и показ окна
            _ = Task.Run(SeedUsersIfEmptyAsync);
        }

        private static async Task SeedUsersIfEmptyAsync()
        {
            try
            {
                using var db = new AppDbContext();
                if (await db.Users.AnyAsync()) return;

                db.Users.Add(new User
                {
                    Username = "admin",
                    PasswordHash = PasswordHelper.HashPassword("admin123"),
                    Role = "Admin"
                });

                db.Users.Add(new User
                {
                    Username = "operator",
                    PasswordHash = PasswordHelper.HashPassword("operator123"),
                    Role = "Operator"
                });

                await db.SaveChangesAsync();
            }
            catch
            {
            }
        }
    }
}

