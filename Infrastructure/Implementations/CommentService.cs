using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TMP.Application.Comments;
using TMP.Application.DTOs.CommentDtos;
using TMP.Application.Interfaces;
using TMPDomain.Entities;
using TMP.Application.Hubs;

namespace TMP.Infrastructure.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<CommentHub> _commentHubContext;

        public CommentService(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<CommentHub> commentHubContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _commentHubContext = commentHubContext;
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsAsync(int? taskId)
        {
            IQueryable<Comment> query = _unitOfWork.Repository<Comment>().GetAll();

            if (taskId.HasValue)
            {
                query = query.Where(c => c.TaskId == taskId.Value);
            }

            var comments = await query.ToListAsync();
            return _mapper.Map<IEnumerable<CommentDto>>(comments);
        }

        public async Task<CommentDto> GetCommentByIdAsync(int id)
        {
            var comment = await _unitOfWork.Repository<Comment>()
                .GetById(c => c.Id == id)
                .FirstOrDefaultAsync();

            if (comment == null) return null;

            return _mapper.Map<CommentDto>(comment);
        }

        public async Task<CommentDto> AddCommentAsync(AddCommentDto newComment, string userId)
        {
            var comment = _mapper.Map<Comment>(newComment);
            comment.UserId = userId;
            comment.CreatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Comment>().Create(comment);
            await _unitOfWork.Repository<Comment>().SaveChangesAsync();

            var commentDto = _mapper.Map<CommentDto>(comment);
            await _commentHubContext.Clients.All.SendAsync("ReceiveComment", commentDto);

            return commentDto;
        }

        public async Task<bool> UpdateCommentAsync(int id, AddCommentDto updatedComment)
        {
            var comment = await _unitOfWork.Repository<Comment>().GetById(c => c.Id == id).FirstOrDefaultAsync();
            if (comment == null) return false;

            _mapper.Map(updatedComment, comment);

            _unitOfWork.Repository<Comment>().Update(comment);
            await _unitOfWork.Repository<Comment>().SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCommentAsync(int id)
        {
            var comment = await _unitOfWork.Repository<Comment>().GetById(c => c.Id == id).FirstOrDefaultAsync();
            if (comment == null) return false;

            _unitOfWork.Repository<Comment>().Delete(comment);
            await _unitOfWork.Repository<Comment>().SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByUserIdAsync(string userId)
        {
            var comments = await _unitOfWork.Repository<Comment>()
                .GetByCondition(c => c.UserId == userId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CommentDto>>(comments);
        }
    }
}
