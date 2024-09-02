// INFORMATION ABOUT THE FILE.

// Here it Manages the Logic for Selecting the Application's Language.

using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace BackupApp.ViewModels
{
    public class LanguageSelectionViewModel : ViewModelBase
    {
        public ICommand SelectLanguageCommand { get; }

        public LanguageSelectionViewModel()
        {
            SelectLanguageCommand = new RelayCommand<string>(SelectLanguage);
        }

        private void SelectLanguage(string languageCode)
        {
            // Set the selected language
            App.SelectedLanguage = languageCode;

            // Close the language selection window
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                var window = desktopLifetime.Windows.FirstOrDefault();
                window?.Close();
            }
        }
    }
}