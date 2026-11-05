using System;
using Avalonia;

namespace CyberClubApp
{
    internal class Program
    {
        // Код инициализации. Не используйте какие-либо объекты Avalonia до вызова BuildAvaloniaApp.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Конфигурация приложения Avalonia.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}

