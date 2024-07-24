using TMP.Application.DTOs.TaskDtos;

namespace TMP.Application.DTOs.ProjectDtos
{
    public class ProjectTasksDto
    {
        public int ProjectId { get; set; }
        public ICollection<TaskDto> Tasks { get; set; } = new List<TaskDto>();
    }
}
