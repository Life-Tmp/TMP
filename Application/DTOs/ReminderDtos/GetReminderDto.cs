namespace TMPApplication.DTOs.ReminderDtos
{
    public class GetReminderDto
    {
        public string Description { get; set; }
        public DateTime ReminderDateTime { get; set; }
        public int TaskId { get; set; }

    }
}
