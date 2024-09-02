// INFORMATION ABOUT THE FILE. 

// Here it provides the code behind functionality for the MainWindow.axaml file including a method to show Notifications.
using Avalonia.Controls;
using Avalonia.Threading;

namespace BackupApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void ShowNotification(string message)
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var dialog = new Window
                {
                    Title = "Notification",
                    Width = 300,
                    Height = 150,
                    Content = new TextBlock
                    {
                        Text = message,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                    }
                };

                await dialog.ShowDialog(this);
            });
        }
    }
}