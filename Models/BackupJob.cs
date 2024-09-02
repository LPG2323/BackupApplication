// INFORMATION ABOUT THE FILE.

// Here it represents the details of a Completed Backup Job, including its name,title,total files and date.

using System;

namespace BackupApp.Models
{
    public class BackupJob
    {
        public string Name { get; set; }
        public DateTime BackupTime { get; set; }
        public int TotalFiles { get; set; }
        public DateTime BackupDate { get; set; }
    }
}