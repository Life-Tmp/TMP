using TMPDomain.Enumerations;

namespace TMP.Application.DTOs.TaskDtos
{
    public class AddTaskDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskPriority Priority { get; set; }
        public StatusOfTask Status { get; set; }
        public DateTime DueDate { get; set; }
        public int ProjectId { get; set; }
        public List<string> Tags { get; set; }
    }
}