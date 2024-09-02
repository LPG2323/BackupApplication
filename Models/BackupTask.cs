// INFORMATION ABOUT THE FILE.

// I have a class called BackupTask that represents a backup task. It has the following properties including its name, source and destination paths, type and priority.

// I have also defined an enum called BackupType that represents the type of backup task. It has 2 values : Full and Differential.

namespace BackupApp.Models
{
    public class BackupTask
    {
        public string Name { get; set; } = string.Empty;
        public string SourcePath { get; set; } = string.Empty;
        public string DestinationPath { get; set; } = string.Empty;
        public BackupType BackupType { get; set; } = BackupType.Full;
        public bool IsPriority { get; set; } = false; // New property
    }

    public enum BackupType
    {
        Full,
        Differential
    }
}