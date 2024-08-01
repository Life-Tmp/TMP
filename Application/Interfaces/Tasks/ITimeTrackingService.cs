namespace TMPApplication.Interfaces.Tasks
{
    public interface ITimeTrackingService
    {
        Task StartTimerAsync(int taskId, string userId);
        Task StopTimerAsync(int taskId, string userId);
        Task<WorkTimeDto> GetTotalTimeSpentAsync(int taskId, string userId);
        Task<IEnumerable<UserTimeSpentDto>> GetTimeSpentByUsersAsync(int taskId, string currentUserId);
    }
}
