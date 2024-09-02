// INFORMATION ABOUT THE FILE. 

// Here it contains the Application-level Initialization logic, setting up the main Window and its Data Context and handling the Application LifeCycle.

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BackupApp.ViewModels;
using BackupApp.Views;

namespace BackupApp
{
    public partial class App : Application
    {
        public static string SelectedLanguage { get; set; } = "en";

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindowViewModel = new MainWindowViewModel();
                var mainWindow = new MainWindow
                {
                    DataContext = mainWindowViewModel,
                };

                desktop.MainWindow = mainWindow;
                mainWindow.Show();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}