using TMPDomain.Enumerations;

namespace TMP.Application.DTOs.TaskDtos
{
    public class UpdateTaskStatusDto
    {
        public int TaskId { get; set; }
        public StatusOfTask Status { get; set; }
    }
}
