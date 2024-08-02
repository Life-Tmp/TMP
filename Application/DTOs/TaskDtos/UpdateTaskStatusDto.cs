using TMPDomain.Enumerations;

namespace TMP.Application.DTOs.TaskDtos
{
    public class UpdateTaskStatusDto
    {
        public string UserId { get; set; }
        public int TaskId { get; set; }
        public StatusOfTask Status { get; set; }
    }
}
