// INFORMATION ABOUT THE FILE.

// Here it Manages the Logic and state for individual backup jobs including Starting, Pausing, Resuming and Stopping Backups.

// It also Saves Backup Job details to a JSON file and Encrypts the JSON content before saving it to a new file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Avalonia.Controls.ApplicationLifetimes;
using BackupApp.Views;
using BackupApp.Helpers;
using BackupApp.Models;

namespace BackupApp.ViewModels
{
    public class BackupJobViewModel : ViewModelBase
    {
        private string _name;
        private string _sourcePath;
        private string _destinationPath;
        private double _backupProgress;
        private int _totalFiles;
        private int _filesBackedUp;
        private CancellationTokenSource _cancellationTokenSource;
        private ManualResetEventSlim _pauseEvent;
        private BackupType _backupType;
        private bool _isPriority;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

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

        public double BackupProgress
        {
            get => _backupProgress;
            set => SetProperty(ref _backupProgress, value);
        }

        public int TotalFiles
        {
            get => _totalFiles;
            set => SetProperty(ref _totalFiles, value);
        }

        public int FilesBackedUp
        {
            get => _filesBackedUp;
            set => SetProperty(ref _filesBackedUp, value);
        }

        public BackupType BackupType
        {
            get => _backupType;
            set => SetProperty(ref _backupType, value);
        }

        public bool IsPriority
        {
            get => _isPriority;
            set => SetProperty(ref _isPriority, value);
        }

        public IEnumerable<BackupType> BackupTypes => Enum.GetValues(typeof(BackupType)) as IEnumerable<BackupType>;

        public ICommand PauseBackupCommand { get; }
        public ICommand ResumeBackupCommand { get; }
        public ICommand StopBackupCommand { get; }

        public BackupJobViewModel()
        {
            PauseBackupCommand = new RelayCommand(PauseBackup);
            ResumeBackupCommand = new RelayCommand(ResumeBackup);
            StopBackupCommand = new RelayCommand(StopBackup);
        }

        public async void StartBackup()
        {
            if (string.IsNullOrEmpty(SourcePath) || string.IsNullOrEmpty(DestinationPath))
            {
                Debug.WriteLine("Source or Destination path is empty.");
                NotifyBackupCompletion(false);
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _pauseEvent = new ManualResetEventSlim(true);

            try
            {
                // Here it Resets progress
                BackupProgress = 0;
                FilesBackedUp = 0;

                // Here it Checks if SourcePath is a file or directory
                if (File.Exists(SourcePath))
                {
                    // Here SourcePath is a file
                    string destFilePath = Path.Combine(DestinationPath, Path.GetFileName(SourcePath));
                    if (File.Exists(destFilePath))
                    {
                        Debug.WriteLine($"The file already exists: {destFilePath}");
                        NotifyBackupCompletion(false);
                        return;
                    }
                    File.Copy(SourcePath, destFilePath);
                    BackupProgress = 100;
                    FilesBackedUp = 1;
                    TotalFiles = 1;
                }
                else if (Directory.Exists(SourcePath))
                {
                    // Here SourcePath is a directory
                    TotalFiles = CountFiles(SourcePath);
                    Debug.WriteLine($"Total files to backup: {TotalFiles}");

                    // Here it Prioritize files if IsPriority is true
                    if (IsPriority)
                    {
                        await Task.Run(() => DirectoryCopyWithPriority(SourcePath, DestinationPath, true, _cancellationTokenSource.Token));
                    }
                    else
                    {
                        await Task.Run(() => DirectoryCopy(SourcePath, DestinationPath, true, _cancellationTokenSource.Token));
                    }
                }
                else
                {
                    Debug.WriteLine("Source path does not exist.");
                    NotifyBackupCompletion(false);
                    return;
                }

                // Here it Saves backup job details to JSON
                SaveBackupJobDetailsToJson();

                // Here it Notifies Completion
                bool success = FilesBackedUp == TotalFiles;
                NotifyBackupCompletion(success);
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Backup was canceled.");
                NotifyBackupCompletion(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                NotifyBackupCompletion(false);
            }
        }

        public void PauseBackup()
        {
            _pauseEvent.Reset();
        }

        public void ResumeBackup()
        {
            _pauseEvent.Set();
        }

        public void StopBackup()
        {
            _cancellationTokenSource?.Cancel();
            Debug.WriteLine("Backup stopped.");
        }

        private void DirectoryCopyWithPriority(string sourceDirName, string destDirName, bool copySubDirs, CancellationToken cancellationToken)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destDirName);

            FileInfo[] files = dir.GetFiles();

            // Here it prioritize PDF files first ( I just choosed PDF files as an example ), then other files
            var prioritizedFiles = files
                .OrderByDescending(f => f.Extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(f => f.Length) // Further prioritize by file size if needed
                .ToArray();

            foreach (FileInfo file in prioritizedFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _pauseEvent.Wait(cancellationToken);

                string temppath = Path.Combine(destDirName, file.Name);
                if (BackupType == BackupType.Differential && File.Exists(temppath) && file.LastWriteTime <= File.GetLastWriteTime(temppath))
                {
                    // Here it Skips files that haven't changed since the Last backup
                    continue;
                }

                file.CopyTo(temppath, true);
                FilesBackedUp++;
                BackupProgress = (double)FilesBackedUp / TotalFiles * 100;
                Debug.WriteLine($"File backed up: {file.Name}, Progress: {BackupProgress}%, FilesBackedUp: {FilesBackedUp}");
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    _pauseEvent.Wait(cancellationToken);

                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopyWithPriority(subdir.FullName, temppath, copySubDirs, cancellationToken);
                }
            }
        }

        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, CancellationToken cancellationToken)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destDirName);

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _pauseEvent.Wait(cancellationToken);

                string temppath = Path.Combine(destDirName, file.Name);
                if (BackupType == BackupType.Differential && File.Exists(temppath) && file.LastWriteTime <= File.GetLastWriteTime(temppath))
                {
                    // Skip files that haven't changed since the last backup
                    continue;
                }

                file.CopyTo(temppath, true);
                FilesBackedUp++;
                BackupProgress = (double)FilesBackedUp / TotalFiles * 100;
                Debug.WriteLine($"File backed up: {file.Name}, Progress: {BackupProgress}%, FilesBackedUp: {FilesBackedUp}");
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    _pauseEvent.Wait(cancellationToken);

                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs, cancellationToken);
                }
            }
        }

        private int CountFiles(string path)
        {
            int fileCount = 0;
            try
            {
                // Here it gets all files in the directory and subdirectories
                string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                fileCount = files.Length;

                // Here it Logs each file found
                foreach (var file in files)
                {
                    Debug.WriteLine($"Found file: {file}");
                }

                Debug.WriteLine($"Counted {fileCount} files in {path}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while counting files: {ex.Message}");
            }
            return fileCount;
        }

        private void SaveBackupJobDetailsToJson()
        {
            string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BackupApp");
            Directory.CreateDirectory(directoryPath); // Here it ensures the directory exists

            string fileName = Path.Combine(directoryPath, "BackupJobs.json");
            List<object> backupJobs;

            if (File.Exists(fileName))
            {
                // Here it reads existing data
                string existingJson = File.ReadAllText(fileName);
                backupJobs = JsonConvert.DeserializeObject<List<object>>(existingJson) ?? new List<object>();
            }
            else
            {
                backupJobs = new List<object>();
            }

            // Here it adds new backup job details with a unique identifier
            var backupJobDetails = new
            {
                Id = Guid.NewGuid(), // Unique identifier
                Name,
                SourcePath,
                DestinationPath,
                BackupTime = DateTime.Now,
                TotalFiles,
                FilesBackedUp,
                BackupType,
                IsPriority // Include priority flag
            };

            backupJobs.Add(backupJobDetails);

            // Save updated data to JSON file
            string json = JsonConvert.SerializeObject(backupJobs, Formatting.Indented);
            File.WriteAllText(fileName, json);

            // Encrypt and save the JSON content
            EncryptAndSaveBackupJobs(json, directoryPath);
        }

        private void EncryptAndSaveBackupJobs(string jsonContent, string directoryPath)
        {
            string encryptedFileName = Path.Combine("/Users/lpg/Desktop", "EncryptedBackupJobs.json");

            try
            {
                // Encrypt the JSON content
                string encryptedContent = EncryptionHelper.EncryptString(jsonContent);
                Debug.WriteLine("Encryption successful.");

                // Save the encrypted content to a new file
                File.WriteAllText(encryptedFileName, encryptedContent);
                Debug.WriteLine($"Encrypted file saved at: {encryptedFileName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred during encryption or saving the file: {ex.Message}");
            }
        }

        private void NotifyBackupCompletion(bool success)
        {
            string message = success ? "Backup completed successfully." : "Backup failed.";
            Debug.WriteLine($"Notification: {message}, FilesBackedUp: {FilesBackedUp}, TotalFiles: {TotalFiles}");

            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (desktop.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.ShowNotification(message);
                }
                else
                {
                    Debug.WriteLine("MainWindow is null.");
                }
            }
            else
            {
                Debug.WriteLine("ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime or is null.");
            }
        }
    }
}