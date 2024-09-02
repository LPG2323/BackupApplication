// INFORMATION ABOUT THE FILE.

// Here it Provides the code behind functionality for the LanguageSelectionWindow.axaml file.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BackupApp.Views
{
    public partial class LanguageSelectionWindow : Window
    {
        public LanguageSelectionWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}