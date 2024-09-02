// INFORMATION ABOUT THE FILE.

// Here it Manages the Logic and state for the Main Window, including Configuring and Managing Backup Tasks and Jobs.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using BackupApp.Models;
using BackupApp.Properties;
using Avalonia.Controls.ApplicationLifetimes;
using BackupApp.Views;

namespace BackupApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _sourcePath = string.Empty;
        private string _destinationPath = string.Empty;
        private string _backupName = string.Empty;
        private BackupType _selectedBackupType = BackupType.Full;
        private bool _isPriority;

        public string SourcePath
        {
            get => _sourcePath;
            set => SetProperty(ref _sourcePath, value);
        }

        public string DestinationPath
        {
            get => _destinationPath;
            set => SetProperty(ref _destinationPath, value);
        }

        public string BackupName
        {
            get => _backupName;
            set => SetProperty(ref _backupName, value);
        }

        public BackupType SelectedBackupType
        {
            get => _selectedBackupType;
            set => SetProperty(ref _selectedBackupType, value);
        }

        public bool IsPriority
        {
            get => _isPriority;
            set => SetProperty(ref _isPriority, value);
        }

        public List<BackupType> BackupTypes { get; } = Enum.GetValues(typeof(BackupType)).Cast<BackupType>().ToList();

        private List<BackupJobViewModel> _backupJobs = new List<BackupJobViewModel>();
        public List<BackupJobViewModel> BackupJobs
        {
            get => _backupJobs;
            set => SetProperty(ref _backupJobs, value);
        }

        public ICommand StartBackupCommand { get; }
        public ICommand PauseBackupCommand { get; }
        public ICommand ResumeBackupCommand { get; }
        public ICommand StopBackupCommand { get; }
        public ICommand ChangeLanguageCommand { get; }

        public MainWindowViewModel()
        {
            StartBackupCommand = new RelayCommand(StartBackup);
            PauseBackupCommand = new RelayCommand(PauseBackup);
            ResumeBackupCommand = new RelayCommand(ResumeBackup);
            StopBackupCommand = new RelayCommand(StopBackup);
            ChangeLanguageCommand = new RelayCommand<string>(ChangeLanguage);

            // Set the initial language based on the selection
            ChangeLanguage(App.SelectedLanguage);
        }

        private void StartBackup()
        {
            var newBackupJob = new BackupJobViewModel
            {
                Name = BackupName,
                SourcePath = SourcePath,
                DestinationPath = DestinationPath,
                BackupType = SelectedBackupType,
                IsPriority = IsPriority // Set priority
            };

            BackupJobs.Add(newBackupJob);
            BackupJobs = new List<BackupJobViewModel>(BackupJobs); // Trigger UI update

            newBackupJob.StartBackup();
        }

        private void PauseBackup()
        {
            var currentJob = BackupJobs.FirstOrDefault();
            if (currentJob != null)
            {
                currentJob.PauseBackup();
                Console.WriteLine("PauseBackupCommand executed.");
            }
        }

        private void ResumeBackup()
        {
            var currentJob = BackupJobs.FirstOrDefault();
            if (currentJob != null)
            {
                currentJob.ResumeBackup();
                Console.WriteLine("ResumeBackupCommand executed.");
            }
        }

        private void StopBackup()
        {
            var currentJob = BackupJobs.FirstOrDefault();
            if (currentJob != null)
            {
                currentJob.StopBackup();
                Console.WriteLine("StopBackupCommand executed.");
            }
        }

        public void ChangeLanguage(string languageCode)
        {
            CultureInfo culture = new CultureInfo(languageCode);
            Resources.Culture = culture;

            // Notify the UI to update the bindings
            OnPropertyChanged(nameof(SourcePathLabel));
            OnPropertyChanged(nameof(DestinationPathLabel));
            OnPropertyChanged(nameof(StartBackupLabel));
        }

        public string SourcePathLabel => Resources.SourcePath;
        public string DestinationPathLabel => Resources.DestinationPath;
        public string StartBackupLabel => Resources.StartBackup;
    }
}