namespace TMPDomain.Entities
{
    public class Subtask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TaskId { get; set; }
        public Task Task { get; set; }
    }
}
