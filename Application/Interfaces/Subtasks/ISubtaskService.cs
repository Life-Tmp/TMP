using TMP.Application.DTOs.SubtaskDtos;

namespace TMPApplication.Interfaces.Subtasks
{
    public interface ISubtaskService
    {
        Task<IEnumerable<SubtaskDto>> GetAllSubtasksAsync();
        Task<SubtaskDto> GetSubtaskByIdAsync(int id);
        Task<SubtaskDto> AddSubtaskAsync(AddSubtaskDto newSubtask);
        Task<bool> UpdateSubtaskAsync(int id, UpdateSubtaskDto updatedSubtask);
        Task<bool> DeleteSubtaskAsync(int id);
        Task<bool> UpdateSubtaskCompletionAsync(UpdateSubtaskCompletionDto dto);
    }
}