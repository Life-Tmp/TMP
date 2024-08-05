using System.Collections.Generic;
using System.Threading.Tasks;
using TMP.Application.DTOs.CommentDtos;

namespace TMPApplication.Interfaces.Comments
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentDto>> GetCommentsAsync();
        Task<CommentDto> GetCommentByIdAsync(int id);
        Task<CommentDto> AddCommentAsync(AddCommentDto newComment, string userId);
        Task<bool> UpdateCommentAsync(int id, AddCommentDto updatedComment);
        Task<bool> DeleteCommentAsync(int id);
        Task<IEnumerable<CommentDto>> GetCommentsByUserIdAsync(string userId);
    }
}