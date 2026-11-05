using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace CyberClubApp
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            await SeedUsersIfEmptyAsync();

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var login = new LoginWindow();
            bool? result = login.ShowDialog();

            if (result == true && login.LoggedInUser is not null)
            {
                var main = new MainWindow(login.LoggedInUser);
                MainWindow = main;
                main.Show();
                ShutdownMode = ShutdownMode.OnMainWindowClose;
            }
            else
            {
                Shutdown();
            }
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

