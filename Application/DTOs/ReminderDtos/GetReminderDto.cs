namespace TMPApplication.DTOs.ReminderDtos
{
    public class GetReminderDto
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime ReminderDateTime { get; set; }
        public int TaskId { get; set; }
    }
}
