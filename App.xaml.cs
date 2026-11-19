using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using course_project_shop_musical_instruments.Data;

namespace course_project_shop_musical_instruments
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Инициализация базы данных при запуске приложения
            try
            {
                using var dbContext = new ApplicationDbContext();
                dbContext.EnsureDatabaseCreated();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Критическая ошибка при инициализации базы данных:\n{ex.Message}\n\nПриложение будет закрыто.",
                    "Ошибка базы данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
                return;
            }
            
            // Предотвращаем завершение приложения при закрытии главного окна
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            
            // Обработка необработанных исключений
            this.DispatcherUnhandledException += (s, args) =>
            {
                System.Diagnostics.Debug.WriteLine($"Unhandled Exception: {args.Exception.Message}\n{args.Exception.StackTrace}");
                MessageBox.Show(
                    $"Необработанная ошибка:\n{args.Exception.Message}\n\n{args.Exception.StackTrace}",
                    "Критическая ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                args.Handled = true; // Предотвращаем завершение приложения
            };
        }
    }

}
