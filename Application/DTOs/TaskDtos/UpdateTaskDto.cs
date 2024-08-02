namespace TMP.Application.DTOs.TaskDtos
{
    public class UpdateTaskDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public int Status { get; set; }
        public DateTime DueDate { get; set; }
        public List<string> Tags { get; set; }
    }
}
