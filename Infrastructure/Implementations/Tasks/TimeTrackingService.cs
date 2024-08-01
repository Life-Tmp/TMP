using Microsoft.EntityFrameworkCore;
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

        public TimeTrackingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task StartTimerAsync(int taskId, string userId)
        {
            // Check if the user is assigned to the task
            var task = await _unitOfWork.Repository<TaskEntity>()
                .GetByCondition(t => t.Id == taskId && t.AssignedUsers.Any(u => u.Id == userId))
                .FirstOrDefaultAsync();

            if (task == null)
            {
                throw new Exception("User is not assigned to this task.");
            }

            // Check if there's already an active timer for this user across any task
            var activeTimeEntry = await _unitOfWork.Repository<TimeTracking>()
                .GetByCondition(tt => tt.UserId == userId && tt.EndTime == null)
                .FirstOrDefaultAsync();

            if (activeTimeEntry != null)
            {
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
        }

        public async Task StopTimerAsync(int taskId, string userId)
        {
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
            }
        }

        public async Task<WorkTimeDto> GetTotalTimeSpentAsync(int taskId, string userId)
        {
            var timeTrackings = await _unitOfWork.Repository<TimeTracking>()
                .GetByCondition(tt => tt.TaskId == taskId && tt.UserId == userId)
                .ToListAsync();

            var totalTicks = timeTrackings.Sum(tt => tt.Duration.Ticks);
            var timeSpan = TimeSpan.FromTicks(totalTicks);


            var workTimeDto = new WorkTimeDto
            {
                WorkTime = timeSpan.ToString(@"dd\:hh\:mm\:ss")
            };

            return workTimeDto;
        }

        public async Task<IEnumerable<UserTimeSpentDto>> GetTimeSpentByUsersAsync(int taskId, string currentUserId)
        {
            // Get the project associated with the task
            var task = await _unitOfWork.Repository<TaskEntity>()
                .GetById(t => t.Id == taskId)
                .Include(t => t.Project)
                .FirstOrDefaultAsync();

            if (task == null)
                throw new Exception("Task not found.");

            // Check if the current user has a TeamLead role in the project
            var projectUser = await _unitOfWork.Repository<ProjectUser>()
                .GetByCondition(pu => pu.ProjectId == task.ProjectId && pu.UserId == currentUserId)
                .FirstOrDefaultAsync();

            if (projectUser == null || projectUser.Role != MemberRole.TeamLead)
                throw new Exception("Unauthorized access: Only TeamLead can view time spent by users.");

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

            return userTimeSpent;
        }
    }
}