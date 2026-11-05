using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace CyberClubApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Асинхронная загрузка списка компьютеров
        private async void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var computers = await db.Computers
                        .AsNoTracking()
                        .Include(c => c.Sessions)
                        .ToListAsync();
                    DgComputers.ItemsSource = computers;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка соединения с Supabase: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Добавление тестового компьютера
        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var newPc = new Computer { Name = "PC-05", IsVip = true };
                    db.Computers.Add(newPc);
                    await db.SaveChangesAsync();
                }

                MessageBox.Show("Компьютер успешно добавлен в облако!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                BtnLoad_Click(sender, e); // обновить таблицу
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка записи: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

