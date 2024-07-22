namespace TMP.Application.DTOs.CommentDtos
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int TaskId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
