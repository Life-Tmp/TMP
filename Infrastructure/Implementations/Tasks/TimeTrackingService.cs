using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMP.Application.Interfaces;
using TMPApplication.Interfaces.Tasks;
using TMPDomain.Entities;
using TaskEntity = TMPDomain.Entities.Task;
using Task = System.Threading.Tasks.Task;
using TMPDomain.Enumerations;

namespace TMPInfrastructure.Implementations.Tasks
{
    public class TimeTrackingService : ITimeTrackingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TimeTrackingService> _logger;

        public TimeTrackingService(IUnitOfWork unitOfWork, ILogger<TimeTrackingService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        #region Create
        public async Task StartTimerAsync(int taskId, string userId)
        {
            _logger.LogInformation("Starting timer for task with ID: {TaskId} by user with ID: {UserId}", taskId, userId);

            // Check if the user is assigned to the task
            var task = await _unitOfWork.Repository<TaskEntity>()
                .GetByCondition(t => t.Id == taskId && t.AssignedUsers.Any(u => u.Id == userId))
                .FirstOrDefaultAsync();

            if (task == null)
            {
                _logger.LogWarning("User with ID: {UserId} is not assigned to task with ID: {TaskId}", userId, taskId);
                throw new Exception("User is not assigned to this task.");
            }

            // Check if there's already an active timer for this user across any task
            var activeTimeEntry = await _unitOfWork.Repository<TimeTracking>()
                .GetByCondition(tt => tt.UserId == userId && tt.EndTime == null)
                .FirstOrDefaultAsync();

            if (activeTimeEntry != null)
            {
                _logger.LogWarning("Active timer already exists for another task for user with ID: {UserId}", userId);
                throw new Exception("An active timer already exists for another task.");
            }

            var timeEntry = new TimeTracking
            {
                TaskId = taskId,
                UserId = userId,
                StartTime = DateTime.UtcNow
            };

            _unitOfWork.Repository<TimeTracking>().Create(timeEntry);
            await _unitOfWork.Repository<TimeTracking>().SaveChangesAsync();

            _logger.LogInformation("Timer started successfully for task with ID: {TaskId} by user with ID: {UserId}", taskId, userId);
        }
        #endregion

        #region Read
        public async Task<WorkTimeDto> GetTotalTimeSpentAsync(int taskId, string userId)
        {
            _logger.LogInformation("Fetching total time spent for task with ID: {TaskId} by user with ID: {UserId}", taskId, userId);

            var timeTrackings = await _unitOfWork.Repository<TimeTracking>()
                .GetByCondition(tt => tt.TaskId == taskId && tt.UserId == userId)
                .ToListAsync();

            var totalTicks = timeTrackings.Sum(tt => tt.Duration.Ticks);
            var timeSpan = TimeSpan.FromTicks(totalTicks);

            var workTimeDto = new WorkTimeDto
            {
                WorkTime = timeSpan.ToString(@"dd\:hh\:mm\:ss")
            };

            _logger.LogInformation("Total time spent fetched successfully for task with ID: {TaskId} by user with ID: {UserId}", taskId, userId);
            return workTimeDto;
        }

        public async Task<IEnumerable<UserTimeSpentDto>> GetTimeSpentByUsersAsync(int taskId, string currentUserId)
        {
            _logger.LogInformation("Fetching time spent by users for task with ID: {TaskId} by user with ID: {UserId}", taskId, currentUserId);

            // Get the project associated with the task
            var task = await _unitOfWork.Repository<TaskEntity>()
                .GetById(t => t.Id == taskId)
                .Include(t => t.Project)
                .FirstOrDefaultAsync();

            if (task == null)
            {
                _logger.LogWarning("Task with ID: {TaskId} not found", taskId);
                throw new Exception("Task not found.");
            }

            // Check if the current user has a TeamLead role in the project
            var projectUser = await _unitOfWork.Repository<ProjectUser>()
                .GetByCondition(pu => pu.ProjectId == task.ProjectId && pu.UserId == currentUserId)
                .FirstOrDefaultAsync();

            if (projectUser == null || projectUser.Role != MemberRole.TeamLead)
            {
                _logger.LogWarning("Unauthorized access: Only TeamLead can view time spent by users for task with ID: {TaskId}", taskId);
                throw new Exception("Unauthorized access: Only TeamLead can view time spent by users.");
            }

            var timeTrackings = await _unitOfWork.Repository<TimeTracking>()
                .GetByCondition(tt => tt.TaskId == taskId)
                .Include(tt => tt.User)
                .ToListAsync();

            var userTimeSpent = timeTrackings
                .GroupBy(tt => tt.UserId)
                .Select(group => new UserTimeSpentDto
                {
                    UserId = group.Key,
                    TaskId = taskId,
                    TimeSpent = TimeSpan.FromTicks(group.Sum(tt => tt.Duration.Ticks)).ToString(@"dd\:hh\:mm\:ss")
                })
                .ToList();

            _logger.LogInformation("Time spent by users fetched successfully for task with ID: {TaskId}", taskId);
            return userTimeSpent;
        }
        #endregion

        #region Update
        public async Task StopTimerAsync(int taskId, string userId)
        {
            _logger.LogInformation("Stopping timer for task with ID: {TaskId} by user with ID: {UserId}", taskId, userId);

            var timeEntry = await _unitOfWork.Repository<TimeTracking>()
                .GetByCondition(t => t.TaskId == taskId && t.UserId == userId && t.EndTime == null)
                .FirstOrDefaultAsync();

            if (timeEntry != null)
            {
                timeEntry.EndTime = DateTime.UtcNow;

                // Calculate the duration and handle nullable Duration
                var duration = (timeEntry.EndTime - timeEntry.StartTime);
                timeEntry.Duration = duration.HasValue ? duration.Value : TimeSpan.Zero;

                _unitOfWork.Repository<TimeTracking>().Update(timeEntry);
                await _unitOfWork.Repository<TimeTracking>().SaveChangesAsync();

                _logger.LogInformation("Timer stopped successfully for task with ID: {TaskId} by user with ID: {UserId}", taskId, userId);
            }
            else
            {
                _logger.LogWarning("No active timer found for task with ID: {TaskId} by user with ID: {UserId}", taskId, userId);
            }
        }
        #endregion
    }
}
