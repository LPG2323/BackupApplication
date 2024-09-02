using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BackupApp.ViewModels;
using System.Linq;

namespace BackupApp.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BackupController : ControllerBase
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly ILogger<BackupController> _logger;

        public BackupController(MainWindowViewModel mainWindowViewModel, ILogger<BackupController> logger)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _logger = logger;
        }

        [HttpPost("start")]
        public IActionResult StartBackup()
        {
            _logger.LogInformation("StartBackup endpoint called.");
            _mainWindowViewModel.StartBackupCommand.Execute(null);
            return Ok("Backup started.");
        }

        [HttpPost("pause")]
        public IActionResult PauseBackup()
        {
            _logger.LogInformation("PauseBackup endpoint called.");
            var currentJob = _mainWindowViewModel.BackupJobs.FirstOrDefault();
            if (currentJob != null)
            {
                currentJob.PauseBackupCommand.Execute(null);
                _logger.LogInformation("Backup paused.");
                return Ok("Backup paused.");
            }
            else
            {
                _logger.LogWarning("No active backup job found to pause.");
                return NotFound("No active backup job found to pause.");
            }
        }

        [HttpPost("resume")]
        public IActionResult ResumeBackup()
        {
            _logger.LogInformation("ResumeBackup endpoint called.");
            var currentJob = _mainWindowViewModel.BackupJobs.FirstOrDefault();
            if (currentJob != null)
            {
                currentJob.ResumeBackupCommand.Execute(null);
                _logger.LogInformation("Backup resumed.");
                return Ok("Backup resumed.");
            }
            else
            {
                _logger.LogWarning("No active backup job found to resume.");
                return NotFound("No active backup job found to resume.");
            }
        }
    }
}